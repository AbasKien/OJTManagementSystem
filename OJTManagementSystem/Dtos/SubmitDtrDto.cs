using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.Dtos
{
    public class SubmitDtrDto
    {
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime RecordDate { get; set; }

        [Required(ErrorMessage = "Time in is required")]
        [DataType(DataType.Time)]
        public TimeSpan TimeIn { get; set; }

        [Required(ErrorMessage = "Time out is required")]
        [DataType(DataType.Time)]
        public TimeSpan TimeOut { get; set; }

        [Required(ErrorMessage = "Activity description is required")]
        [StringLength(500, MinimumLength = 10)]
        public string ActivityDescription { get; set; }
    }
}