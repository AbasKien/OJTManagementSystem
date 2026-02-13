using System.ComponentModel.DataAnnotations;
using OJTManagementSystem.Enums;

namespace OJTManagementSystem.ViewModel
{
    public class SubmitLeaveRequestViewModel
    {
        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Leave type is required")]
        [Display(Name = "Leave Type")]
        public LeaveType LeaveType { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters")]
        [Display(Name = "Reason for Leave")]
        [DataType(DataType.MultilineText)]
        public string Reason { get; set; } = string.Empty;

        // Computed property to calculate number of days
        public int TotalDays
        {
            get
            {
                if (EndDate >= StartDate)
                {
                    return (EndDate - StartDate).Days + 1;
                }
                return 0;
            }
        }

        // Helper property for validation message
        public bool IsValidDateRange => EndDate >= StartDate;

        // Helper property to display leave type options
        public static Dictionary<LeaveType, string> LeaveTypeOptions => new Dictionary<LeaveType, string>
        {
            { LeaveType.Sick, "Sick Leave" },
            { LeaveType.Personal, "Personal Leave" },
            { LeaveType.Emergency, "Emergency Leave" },
            { LeaveType.Vacation, "Vacation Leave" }
        };
    }
}