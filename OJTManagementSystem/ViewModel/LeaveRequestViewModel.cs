using OJTManagementSystem.Enums;

namespace OJTManagementSystem.ViewModel
{
    public class LeaveRequestViewModel
    {
        public int LeaveRequestId { get; set; }
        public int InternId { get; set; }
        public string InternName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Reason { get; set; }
        public LeaveStatus Status { get; set; }
        public string RejectionReason { get; set; }
        public int NumberOfDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}