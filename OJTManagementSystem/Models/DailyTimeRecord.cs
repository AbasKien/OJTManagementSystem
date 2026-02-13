using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OJTManagementSystem.Enums;

namespace OJTManagementSystem.Models
{
    public class DailyTimeRecord
    {
        [Key]
        public int DtrId { get; set; }

        [Required]
        public int InternId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime RecordDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan TimeIn { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan TimeOut { get; set; }

        [Required]
        [StringLength(500)]
        public string ActivityDescription { get; set; }

        [Range(0, 24)]
        public decimal TotalHours { get; set; }

        public DtrStatus Status { get; set; } = DtrStatus.Pending;

        // ✅ FIXED: Made nullable - RejectionReason is only filled when Status = Rejected
        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [ForeignKey(nameof(InternId))]
        public virtual Intern Intern { get; set; }
    }
}