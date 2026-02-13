using System.ComponentModel.DataAnnotations;
using OJTManagementSystem.Enums;

namespace OJTManagementSystem.ViewModel
{
    /// <summary>
    /// ViewModel for the DTR approval page (combines DTR details + approval form)
    /// </summary>
    public class ApproveDtrViewModel
    {
        // ========================================
        // DTR DETAILS (Read-only display)
        // ========================================
        public int DtrId { get; set; }
        public int InternId { get; set; }
        public string InternName { get; set; }
        public DateTime RecordDate { get; set; }
        public TimeSpan TimeIn { get; set; }
        public TimeSpan TimeOut { get; set; }
        public string ActivityDescription { get; set; }
        public decimal TotalHours { get; set; }
        public DtrStatus CurrentStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        // ========================================
        // APPROVAL FORM (User input)
        // ========================================
        [Required(ErrorMessage = "Please select a status")]
        [Display(Name = "Decision")]
        public DtrStatus Status { get; set; }

        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }

        // ========================================
        // HELPER PROPERTIES
        // ========================================
        public bool IsApproved => Status == DtrStatus.Approved;
        public bool IsRejected => Status == DtrStatus.Rejected;
    }
}