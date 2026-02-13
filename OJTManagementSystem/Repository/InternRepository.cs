using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    public class InternRepository : GenericRepository<Intern>, IInternRepository
    {
        private readonly ApplicationDbContext _context;

        public InternRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Intern> GetByUserIdAsync(string userId)
        {
            return await _context.Interns
                .Include(i => i.User)
                .Include(i => i.Supervisor)
                    .ThenInclude(s => s.User)   // ✅ FIX: loads Supervisor.User so FullName is available
                .FirstOrDefaultAsync(i => i.UserId == userId);
        }

        public async Task<List<Intern>> GetBySupervisorIdAsync(int supervisorId)
        {
            return await _context.Interns
                .Where(i => i.SupervisorId == supervisorId)
                .Include(i => i.User)  // ✅ CRITICAL: Load User data
                .Include(i => i.Supervisor)
                    .ThenInclude(s => s.User)  // ✅ CRITICAL: Load Supervisor.User data
                .Include(i => i.DailyTimeRecords)
                .Include(i => i.LeaveRequests)
                .ToListAsync();
        }

        public async Task<Intern> GetWithAllDataAsync(int internId)
        {
            return await _context.Interns
                .Include(i => i.User)
                .Include(i => i.Supervisor)
                    .ThenInclude(s => s.User)   // ✅ FIX: loads Supervisor.User so FullName is available
                .Include(i => i.DailyTimeRecords)
                .Include(i => i.Evaluations)
                .Include(i => i.Certificates)
                .Include(i => i.LeaveRequests)
                .FirstOrDefaultAsync(i => i.InternId == internId);
        }
    }
}