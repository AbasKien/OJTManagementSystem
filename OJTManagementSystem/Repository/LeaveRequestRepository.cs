using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Enums;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<LeaveRequest>> GetByInternIdAsync(int internId)
        {
            return await _context.LeaveRequests
                .Where(l => l.InternId == internId)
                .Include(l => l.Intern)
                    .ThenInclude(i => i.User)  // ✅ Include User to get FullName
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetByStatusAsync(LeaveStatus status)
        {
            return await _context.LeaveRequests
                .Where(l => l.Status == status)
                .Include(l => l.Intern)
                    .ThenInclude(i => i.User)  // ✅ Include User to get FullName
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetPendingAsync()
        {
            return await _context.LeaveRequests
                .Where(l => l.Status == LeaveStatus.Pending)
                .Include(l => l.Intern)
                    .ThenInclude(i => i.User)  // ✅ Include User to get FullName
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetApprovedAsync()
        {
            return await _context.LeaveRequests
                .Where(l => l.Status == LeaveStatus.Approved)
                .Include(l => l.Intern)
                    .ThenInclude(i => i.User)  // ✅ Include User to get FullName
                .ToListAsync();
        }

        public async Task<LeaveRequest> GetByIdWithInternAsync(int leaveRequestId)
        {
            return await _context.LeaveRequests
                .Include(l => l.Intern)
                    .ThenInclude(i => i.User)  // ✅ Include User to get FullName
                .FirstOrDefaultAsync(l => l.LeaveRequestId == leaveRequestId);
        }
    }
}