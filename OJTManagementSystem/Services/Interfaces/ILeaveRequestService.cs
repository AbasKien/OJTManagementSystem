using OJTManagementSystem.Dtos;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Services.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequestViewModel> SubmitLeaveRequestAsync(int internId, SubmitLeaveRequestDto dto);
        Task<LeaveRequestViewModel> ApproveLeaveRequestAsync(ApproveLeaveRequestDto dto);
        Task<LeaveRequestViewModel> GetLeaveRequestByIdAsync(int leaveRequestId);
        Task<List<LeaveRequestViewModel>> GetInternLeaveRequestsAsync(int internId);
        Task<List<LeaveRequestViewModel>> GetPendingLeaveRequestsAsync();
        Task DeleteLeaveRequestAsync(int leaveRequestId);
    }
}