using OJTManagementSystem.Enums;

namespace OJTManagementSystem.Models
{
    public class LeaveRequest
    {
        public int LeaveRequestId { get; set; }
        public int InternId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public LeaveType LeaveType { get; set; }
        public string Reason { get; set; }
        public int NumberOfDays { get; set; }

        public LeaveStatus Status { get; set; }

        public string Remarks { get; set; }
        public string RejectionReason { get; set; }

        // ✅ FIXED: Made nullable - only set when a supervisor approves the request
        // When a new leave request is submitted in Pending status, this field will be null
        // It's populated only when the request is Approved
        public string ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        // ✅ FIXED: Changed from read-only to read-write property
        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual Intern Intern { get; set; }
    }
}