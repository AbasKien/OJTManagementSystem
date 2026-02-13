using System.ComponentModel.DataAnnotations;
using OJTManagementSystem.Enums;

namespace OJTManagementSystem.Dtos
{
    public class ApproveLeaveRequestDto
    {
        [Required(ErrorMessage = "Leave request ID is required")]
        public int LeaveRequestId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public LeaveStatus Status { get; set; }

        // ✅ NEW: ApprovedBy field - set when supervisor approves the request
        [StringLength(255, ErrorMessage = "Approver name cannot exceed 255 characters")]
        public string ApprovedBy { get; set; }

        // ✅ Rejection reason - only required when Status is Rejected
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string RejectionReason { get; set; }
    }
}