using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    public interface ITimeRecordRepository
    {
        Task<IEnumerable<DailyTimeRecord>> GetAllTimeRecordsAsync();
        Task<DailyTimeRecord?> GetTimeRecordByIdAsync(int id);
        Task<IEnumerable<DailyTimeRecord>> GetTimeRecordsByInternIdAsync(int internId);
        Task<bool> AddTimeRecordAsync(DailyTimeRecord timeRecord);
        Task<bool> UpdateTimeRecordAsync(DailyTimeRecord timeRecord);
        Task<bool> DeleteTimeRecordAsync(int id);
    }
}