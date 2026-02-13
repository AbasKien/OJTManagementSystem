using System.ComponentModel.DataAnnotations;
using OJTManagementSystem.Enums;

namespace OJTManagementSystem.ViewModel
{
    /// <summary>
    /// ViewModel for the Leave Request approval page (combines leave details + approval form)
    /// </summary>
    public class ApproveLeaveRequestViewModel
    {
        // ========================================
        // LEAVE REQUEST DETAILS (Read-only display)
        // ========================================
        public int LeaveRequestId { get; set; }
        public int InternId { get; set; }
        public string InternName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Reason { get; set; }
        public int NumberOfDays { get; set; }
        public LeaveStatus CurrentStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        // ========================================
        // APPROVAL FORM (User input)
        // ========================================
        [Required(ErrorMessage = "Please select a status")]
        [Display(Name = "Decision")]
        public LeaveStatus Status { get; set; }

        [StringLength(255, ErrorMessage = "Approver name cannot exceed 255 characters")]
        [Display(Name = "Approved By")]
        public string ApprovedBy { get; set; }

        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }

        // ========================================
        // HELPER PROPERTIES
        // ========================================
        public bool IsApproved => Status == LeaveStatus.Approved;
        public bool IsRejected => Status == LeaveStatus.Rejected;

        public string LeaveTypeDisplay => LeaveType switch
        {
            LeaveType.Sick => "Sick Leave",
            LeaveType.Personal => "Personal Leave",
            LeaveType.Emergency => "Emergency Leave",
            LeaveType.Vacation => "Vacation Leave",
            _ => LeaveType.ToString()
        };
    }
}