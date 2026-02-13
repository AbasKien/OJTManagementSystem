using OJTManagementSystem.Enums;

namespace OJTManagementSystem.ViewModel
{
    public class DailyTimeRecordViewModel
    {
        public int DtrId { get; set; }
        public int InternId { get; set; }
        public string InternName { get; set; }
        public DateTime RecordDate { get; set; }
        public DateTime Date => RecordDate;
        public TimeSpan TimeIn { get; set; }
        public TimeSpan TimeOut { get; set; }
        public string ActivityDescription { get; set; }
        public string Activities => ActivityDescription;
        public decimal TotalHours { get; set; }
        public DtrStatus Status { get; set; }
        public string RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}