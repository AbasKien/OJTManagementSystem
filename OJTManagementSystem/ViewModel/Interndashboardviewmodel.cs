namespace OJTManagementSystem.ViewModel
{
    public class InternDashboardViewModel
    {
        public InternViewModel InternProfile { get; set; }
        public int TotalDtrSubmitted { get; set; }
        public int ApprovedDtrCount { get; set; }
        public int PendingDtrCount { get; set; }
        public int RejectedDtrCount { get; set; }
        public decimal TotalHoursRendered { get; set; }
        public int ApprovedLeaveCount { get; set; }
        public int PendingLeaveCount { get; set; }
        public EvaluationViewModel LatestEvaluation { get; set; }
        public CertificateViewModel Certificate { get; set; }
        public List<DailyTimeRecordViewModel> RecentDtrs { get; set; } = new List<DailyTimeRecordViewModel>();
    }
}