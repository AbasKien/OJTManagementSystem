using OJTManagementSystem.Models;

public interface ISupervisorService
{
    Task<Supervisor?> GetSupervisorByUserIdAsync(string userId);
    Task<IEnumerable<Supervisor>> GetAllActiveSupervisorsAsync();  // ✅ ADDED
    Task<IEnumerable<Intern>> GetSupervisorInternsAsync(int supervisorId);
    Task<IEnumerable<DailyTimeRecord>> GetPendingTimeRecordsAsync(int supervisorId);
    Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync(int supervisorId);
    Task<bool> ApproveTimeRecordAsync(int timeRecordId, string approvedBy);
    Task<bool> RejectTimeRecordAsync(int timeRecordId, string remarks);
    Task<bool> ApproveLeaveRequestAsync(int leaveRequestId, string approvedBy);
    Task<bool> RejectLeaveRequestAsync(int leaveRequestId, string remarks);
    Task<bool> CreateEvaluationAsync(Evaluation evaluation);
    Task<bool> GenerateCertificateAsync(int internId, string issuedBy);
}