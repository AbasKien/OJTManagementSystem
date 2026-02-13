using OJTManagementSystem.Dtos;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Services.Interfaces
{
    public interface IDtrService
    {
        Task<DailyTimeRecordViewModel> SubmitDtrAsync(int internId, SubmitDtrDto dto);
        Task<DailyTimeRecordViewModel> ApproveDtrAsync(ApproveDtrDto dto);
        Task<List<DailyTimeRecordViewModel>> GetInternDtrsAsync(int internId);
        Task<List<DailyTimeRecordViewModel>> GetPendingDtrsAsync();
        Task<DailyTimeRecordViewModel> GetDtrByIdAsync(int dtrId);
        Task DeleteDtrAsync(int dtrId);
        Task<decimal> GetTotalHoursByInternAsync(int internId);
    }
}