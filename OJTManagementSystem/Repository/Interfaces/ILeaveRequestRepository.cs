using OJTManagementSystem.Enums;
using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
    {
        Task<List<LeaveRequest>> GetByInternIdAsync(int internId);
        Task<List<LeaveRequest>> GetByStatusAsync(LeaveStatus status);
        Task<List<LeaveRequest>> GetPendingAsync();
        Task<List<LeaveRequest>> GetApprovedAsync();
        Task<LeaveRequest> GetByIdWithInternAsync(int leaveRequestId);
    }
}