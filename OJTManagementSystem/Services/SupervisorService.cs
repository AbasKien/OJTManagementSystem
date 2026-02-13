using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Models;
using OJTManagementSystem.Services.Interfaces;
using OJTManagementSystem.Enums;

namespace OJTManagementSystem.Services
{
    public class SupervisorService : ISupervisorService
    {
        private readonly ApplicationDbContext _context;

        public SupervisorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Supervisor?> GetSupervisorByUserIdAsync(string userId)
        {
            return await _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Interns)
                .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // ✅ FIXED: Changed OrderBy from FullName (computed property) to FirstName/LastName (database columns)
        public async Task<IEnumerable<Supervisor>> GetAllActiveSupervisorsAsync()
        {
            return await _context.Supervisors
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .OrderBy(s => s.User.FirstName)      // ✅ Database column
                .ThenBy(s => s.User.LastName)        // ✅ Database column
                .ToListAsync();
        }

        public async Task<IEnumerable<Intern>> GetSupervisorInternsAsync(int supervisorId)
        {
            return await _context.Interns
                .Include(i => i.User)
                .Where(i => i.SupervisorId == supervisorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DailyTimeRecord>> GetPendingTimeRecordsAsync(int supervisorId)
        {
            var internIds = await _context.Interns
                .Where(i => i.SupervisorId == supervisorId)
                .Select(i => i.InternId)
                .ToListAsync();

            return await _context.DailyTimeRecords
                .Include(t => t.Intern)
                .ThenInclude(i => i.User)
                .Where(t => internIds.Contains(t.InternId) && t.Status == DtrStatus.Pending)
                .OrderByDescending(t => t.RecordDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync(int supervisorId)
        {
            var internIds = await _context.Interns
                .Where(i => i.SupervisorId == supervisorId)
                .Select(i => i.InternId)
                .ToListAsync();

            return await _context.LeaveRequests
                .Include(l => l.Intern)
                .ThenInclude(i => i.User)
                .Where(l => internIds.Contains(l.InternId) && l.Status == LeaveStatus.Pending)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveTimeRecordAsync(int timeRecordId, string approvedBy)
        {
            try
            {
                var timeRecord = await _context.DailyTimeRecords.FindAsync(timeRecordId);
                if (timeRecord == null) return false;

                timeRecord.Status = DtrStatus.Approved;
                timeRecord.ApprovedAt = DateTime.UtcNow;

                // Update intern's completed hours
                var intern = await _context.Interns.FindAsync(timeRecord.InternId);
                if (intern != null)
                {
                    intern.CompletedHours += (decimal)timeRecord.TotalHours;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectTimeRecordAsync(int timeRecordId, string remarks)
        {
            try
            {
                var timeRecord = await _context.DailyTimeRecords.FindAsync(timeRecordId);
                if (timeRecord == null) return false;

                timeRecord.Status = DtrStatus.Rejected;
                timeRecord.RejectionReason = remarks;
                timeRecord.ApprovedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApproveLeaveRequestAsync(int leaveRequestId, string approvedBy)
        {
            try
            {
                var leaveRequest = await _context.LeaveRequests.FindAsync(leaveRequestId);
                if (leaveRequest == null) return false;

                leaveRequest.Status = LeaveStatus.Approved;
                leaveRequest.ApprovedBy = approvedBy;
                leaveRequest.ApprovedDate = DateTime.UtcNow;
                leaveRequest.ApprovedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectLeaveRequestAsync(int leaveRequestId, string remarks)
        {
            try
            {
                var leaveRequest = await _context.LeaveRequests.FindAsync(leaveRequestId);
                if (leaveRequest == null) return false;

                leaveRequest.Status = LeaveStatus.Rejected;
                leaveRequest.Remarks = remarks;
                leaveRequest.ApprovedDate = DateTime.UtcNow;
                leaveRequest.ApprovedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateEvaluationAsync(Evaluation evaluation)
        {
            try
            {
                _context.Evaluations.Add(evaluation);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> GenerateCertificateAsync(int internId, string issuedBy)
        {
            try
            {
                var intern = await _context.Interns.FindAsync(internId);
                if (intern == null) return false;

                var certificate = new Certificate
                {
                    InternId = internId,
                    CertificateNumber = $"OJT-{DateTime.Now.Year}-{internId:D4}",
                    IssuedDate = DateTime.Now,
                    IssueDate = DateTime.Now,
                    TotalHoursRendered = intern.CompletedHours,
                    TotalHoursCompleted = intern.CompletedHours,
                    IssuedBy = issuedBy
                };

                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}