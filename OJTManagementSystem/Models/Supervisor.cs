using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OJTManagementSystem.Models
{
    public class Supervisor
    {
        [Key]
        public int SupervisorId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(200)]
        public string Position { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [StringLength(200)]
        public string Department { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Intern> Interns { get; set; } = new List<Intern>();
        public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    }
}