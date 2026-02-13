using OJTManagementSystem.Enums;
using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    public interface IDailyTimeRecordRepository : IGenericRepository<DailyTimeRecord>
    {
        Task<List<DailyTimeRecord>> GetByInternIdAsync(int internId);
        Task<List<DailyTimeRecord>> GetByStatusAsync(DtrStatus status);
        Task<List<DailyTimeRecord>> GetPendingByInternAsync(int internId);
        Task<DailyTimeRecord> GetByDateAsync(int internId, DateTime date);
        Task<decimal> GetTotalHoursByInternAsync(int internId);
        Task<List<DailyTimeRecord>> GetByDateRangeAsync(int internId, DateTime startDate, DateTime endDate);
    }
}