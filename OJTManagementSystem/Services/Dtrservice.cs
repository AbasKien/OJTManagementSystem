using OJTManagementSystem.Dtos;
using OJTManagementSystem.Enums;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.ViewModel;
using OJTManagementSystem.Services.Interfaces;

namespace OJTManagementSystem.Services
{
    public class DtrService : IDtrService
    {
        private readonly IDailyTimeRecordRepository _dtrRepository;
        private readonly IInternRepository _internRepository;

        public DtrService(
            IDailyTimeRecordRepository dtrRepository,
            IInternRepository internRepository)
        {
            _dtrRepository = dtrRepository;
            _internRepository = internRepository;
        }

        public async Task<DailyTimeRecordViewModel> SubmitDtrAsync(int internId, SubmitDtrDto dto)
        {
            var intern = await _internRepository.GetByIdAsync(internId);
            if (intern == null)
                throw new Exception("Intern not found");

            var existingDtr = await _dtrRepository.GetByDateAsync(internId, dto.RecordDate);
            if (existingDtr != null)
                throw new Exception("DTR already exists for this date");

            if (dto.TimeOut <= dto.TimeIn)
                throw new Exception("Time Out must be after Time In");

            var totalHours = CalculateTotalHours(dto.TimeIn, dto.TimeOut);

            var dtr = new DailyTimeRecord
            {
                InternId = internId,
                RecordDate = dto.RecordDate.Date,
                TimeIn = dto.TimeIn,
                TimeOut = dto.TimeOut,
                ActivityDescription = dto.ActivityDescription,
                TotalHours = totalHours,
                Status = DtrStatus.Pending,
                RejectionReason = string.Empty,  // ✅ WORKAROUND: Set empty string instead of null
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                ApprovedAt = null
            };

            await _dtrRepository.AddAsync(dtr);
            return MapToViewModel(dtr);
        }

        public async Task<DailyTimeRecordViewModel> ApproveDtrAsync(ApproveDtrDto dto)
        {
            var dtr = await _dtrRepository.GetByIdAsync(dto.DtrId);
            if (dtr == null)
                throw new Exception("DTR not found");

            dtr.Status = dto.Status;
            dtr.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == DtrStatus.Approved)
            {
                dtr.ApprovedAt = DateTime.UtcNow;
                dtr.RejectionReason = string.Empty;  // ✅ WORKAROUND: Set empty string
            }
            else if (dto.Status == DtrStatus.Rejected)
            {
                dtr.RejectionReason = dto.RejectionReason ?? string.Empty;  // ✅ WORKAROUND: Ensure not null
                dtr.ApprovedAt = null;
            }

            await _dtrRepository.UpdateAsync(dtr);
            return MapToViewModel(dtr);
        }

        public async Task<List<DailyTimeRecordViewModel>> GetInternDtrsAsync(int internId)
        {
            var dtrs = await _dtrRepository.GetByInternIdAsync(internId);
            return dtrs.Select(MapToViewModel).ToList();
        }

        public async Task<List<DailyTimeRecordViewModel>> GetPendingDtrsAsync()
        {
            var dtrs = await _dtrRepository.GetByStatusAsync(DtrStatus.Pending);
            return dtrs.Select(MapToViewModel).ToList();
        }

        public async Task<DailyTimeRecordViewModel> GetDtrByIdAsync(int dtrId)
        {
            var dtr = await _dtrRepository.GetByIdAsync(dtrId);
            if (dtr == null)
                throw new Exception("DTR not found");

            return MapToViewModel(dtr);
        }

        public async Task DeleteDtrAsync(int dtrId)
        {
            var dtr = await _dtrRepository.GetByIdAsync(dtrId);
            if (dtr == null)
                throw new Exception("DTR not found");

            await _dtrRepository.DeleteAsync(dtr);
        }

        public async Task<decimal> GetTotalHoursByInternAsync(int internId)
        {
            return await _dtrRepository.GetTotalHoursByInternAsync(internId);
        }

        private decimal CalculateTotalHours(TimeSpan timeIn, TimeSpan timeOut)
        {
            var duration = timeOut - timeIn;

            if (duration.TotalSeconds < 0)
                duration = duration.Add(TimeSpan.FromHours(24));

            return (decimal)duration.TotalHours;
        }

        private DailyTimeRecordViewModel MapToViewModel(DailyTimeRecord dtr)
        {
            return new DailyTimeRecordViewModel
            {
                DtrId = dtr.DtrId,
                InternId = dtr.InternId,
                InternName = dtr.Intern?.User?.FullName ?? "Unknown",
                RecordDate = dtr.RecordDate,
                TimeIn = dtr.TimeIn,
                TimeOut = dtr.TimeOut,
                ActivityDescription = dtr.ActivityDescription,
                TotalHours = dtr.TotalHours,
                Status = dtr.Status,
                RejectionReason = dtr.RejectionReason,
                CreatedAt = dtr.CreatedAt,
                UpdatedAt = dtr.UpdatedAt,
                ApprovedAt = dtr.ApprovedAt
            };
        }
    }
}