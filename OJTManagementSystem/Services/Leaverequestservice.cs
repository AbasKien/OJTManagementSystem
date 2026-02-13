using OJTManagementSystem.Dtos;
using OJTManagementSystem.Enums;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.ViewModel;
using OJTManagementSystem.Services.Interfaces;

namespace OJTManagementSystem.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _leaveRepository;
        private readonly IInternRepository _internRepository;

        public LeaveRequestService(
            ILeaveRequestRepository leaveRepository,
            IInternRepository internRepository)
        {
            _leaveRepository = leaveRepository;
            _internRepository = internRepository;
        }

        /// <summary>
        /// Submit a new leave request in Pending status
        /// </summary>
        public async Task<LeaveRequestViewModel> SubmitLeaveRequestAsync(int internId, SubmitLeaveRequestDto dto)
        {
            var intern = await _internRepository.GetByIdAsync(internId);
            if (intern == null)
                throw new Exception("Intern not found");

            if (dto.EndDate < dto.StartDate)
                throw new Exception("End date must be after start date");

            var numberOfDays = (int)(dto.EndDate - dto.StartDate).TotalDays + 1;

            var leaveRequest = new LeaveRequest
            {
                InternId = internId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                LeaveType = dto.LeaveType,
                Reason = dto.Reason,
                NumberOfDays = numberOfDays,
                Status = LeaveStatus.Pending,
                Remarks = string.Empty,  // ✅ Set empty string for non-nullable column
                RejectionReason = string.Empty,  // ✅ Set empty string for non-nullable column
                ApprovedBy = string.Empty  // ✅ Set empty string (in case DB column is NOT NULL)
                // ApprovedAt is null - will be set only when approved
            };

            await _leaveRepository.AddAsync(leaveRequest);
            return await GetLeaveRequestByIdAsync(leaveRequest.LeaveRequestId);
        }

        /// <summary>
        /// Approve or reject a leave request
        /// </summary>
        public async Task<LeaveRequestViewModel> ApproveLeaveRequestAsync(ApproveLeaveRequestDto dto)
        {
            var leaveRequest = await _leaveRepository.GetByIdAsync(dto.LeaveRequestId);
            if (leaveRequest == null)
                throw new Exception("Leave request not found");

            leaveRequest.Status = dto.Status;
            leaveRequest.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == LeaveStatus.Approved)
            {
                // ✅ Only set ApprovedAt and ApprovedBy when actually approving
                leaveRequest.ApprovedAt = DateTime.UtcNow;
                leaveRequest.ApprovedBy = dto.ApprovedBy; // Set the approver's name/ID
            }
            else if (dto.Status == LeaveStatus.Rejected)
            {
                // When rejected, ApprovedAt and ApprovedBy remain null
                leaveRequest.RejectionReason = dto.RejectionReason;
            }

            await _leaveRepository.UpdateAsync(leaveRequest);
            return await GetLeaveRequestByIdAsync(leaveRequest.LeaveRequestId);
        }

        public async Task<LeaveRequestViewModel> GetLeaveRequestByIdAsync(int leaveRequestId)
        {
            // ✅ Use GetByIdWithInternAsync instead of GetByIdAsync to include Intern.User
            var leaveRequest = await _leaveRepository.GetByIdWithInternAsync(leaveRequestId);
            if (leaveRequest == null)
                throw new Exception("Leave request not found");

            return MapToViewModel(leaveRequest);
        }

        public async Task<List<LeaveRequestViewModel>> GetInternLeaveRequestsAsync(int internId)
        {
            var leaveRequests = await _leaveRepository.GetByInternIdAsync(internId);
            return leaveRequests.Select(MapToViewModel).ToList();
        }

        public async Task<List<LeaveRequestViewModel>> GetPendingLeaveRequestsAsync()
        {
            var leaveRequests = await _leaveRepository.GetPendingAsync();
            return leaveRequests.Select(MapToViewModel).ToList();
        }

        public async Task DeleteLeaveRequestAsync(int leaveRequestId)
        {
            var leaveRequest = await _leaveRepository.GetByIdAsync(leaveRequestId);
            if (leaveRequest == null)
                throw new Exception("Leave request not found");

            await _leaveRepository.DeleteAsync(leaveRequest);
        }

        /// <summary>
        /// Map LeaveRequest entity to LeaveRequestViewModel
        /// Handles null values appropriately
        /// </summary>
        private LeaveRequestViewModel MapToViewModel(LeaveRequest leaveRequest)
        {
            return new LeaveRequestViewModel
            {
                LeaveRequestId = leaveRequest.LeaveRequestId,
                InternId = leaveRequest.InternId,
                InternName = leaveRequest.Intern?.User?.FullName,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                LeaveType = leaveRequest.LeaveType,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status,
                RejectionReason = leaveRequest.RejectionReason,
                NumberOfDays = leaveRequest.NumberOfDays,
                CreatedAt = leaveRequest.CreatedAt,
                UpdatedAt = leaveRequest.UpdatedAt,
                ApprovedAt = leaveRequest.ApprovedAt
                // Note: ApprovedBy is intentionally not mapped to ViewModel
                // If needed in the future, add it to LeaveRequestViewModel as nullable string
            };
        }
    }
}