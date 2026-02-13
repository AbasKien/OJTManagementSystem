using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.ViewModel
{
    public class SubmitDtrViewModel
    {
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Time in is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Time In")]
        public TimeSpan TimeIn { get; set; }

        [Required(ErrorMessage = "Time out is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Time Out")]
        public TimeSpan TimeOut { get; set; }

        [Required(ErrorMessage = "Activity description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Activity description must be between 10 and 500 characters")]
        [Display(Name = "Activity Description")]
        [DataType(DataType.MultilineText)]
        public string ActivityDescription { get; set; } = string.Empty;

        // Computed property to show total hours worked
        public decimal TotalHours
        {
            get
            {
                if (TimeOut > TimeIn)
                {
                    var duration = TimeOut - TimeIn;
                    return (decimal)duration.TotalHours;
                }
                return 0;
            }
        }

        // Helper property to display formatted time
        public string FormattedTimeIn => TimeIn.ToString(@"hh\:mm");
        public string FormattedTimeOut => TimeOut.ToString(@"hh\:mm");
    }
}