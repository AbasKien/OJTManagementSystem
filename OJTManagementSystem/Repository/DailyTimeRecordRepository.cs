using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Enums;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    public class DailyTimeRecordRepository : GenericRepository<DailyTimeRecord>, IDailyTimeRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public DailyTimeRecordRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<DailyTimeRecord>> GetByInternIdAsync(int internId)
        {
            return await _context.DailyTimeRecords
                .Where(d => d.InternId == internId)
                .Include(d => d.Intern)
                    .ThenInclude(i => i.User)  // ✅ Added ThenInclude for User
                .OrderByDescending(d => d.RecordDate)
                .ToListAsync();
        }

        public async Task<List<DailyTimeRecord>> GetByStatusAsync(DtrStatus status)
        {
            return await _context.DailyTimeRecords
                .Where(d => d.Status == status)
                .Include(d => d.Intern)
                    .ThenInclude(i => i.User)  // ✅ Added ThenInclude for User
                .OrderByDescending(d => d.RecordDate)
                .ToListAsync();
        }

        public async Task<List<DailyTimeRecord>> GetPendingByInternAsync(int internId)
        {
            return await _context.DailyTimeRecords
                .Where(d => d.InternId == internId && d.Status == DtrStatus.Pending)
                .OrderByDescending(d => d.RecordDate)
                .ToListAsync();
        }

        public async Task<DailyTimeRecord> GetByDateAsync(int internId, DateTime date)
        {
            return await _context.DailyTimeRecords
                .FirstOrDefaultAsync(d => d.InternId == internId && d.RecordDate.Date == date.Date);
        }

        public async Task<decimal> GetTotalHoursByInternAsync(int internId)
        {
            var total = await _context.DailyTimeRecords
                .Where(d => d.InternId == internId && d.Status == DtrStatus.Approved)
                .SumAsync(d => (decimal?)d.TotalHours);  // ✅ Cast to nullable to handle empty results

            return total ?? 0m;  // ✅ Return 0 if no approved DTRs
        }

        public async Task<List<DailyTimeRecord>> GetByDateRangeAsync(int internId, DateTime startDate, DateTime endDate)
        {
            return await _context.DailyTimeRecords
                .Where(d => d.InternId == internId && d.RecordDate >= startDate && d.RecordDate <= endDate)
                .OrderByDescending(d => d.RecordDate)
                .ToListAsync();
        }
    }
}