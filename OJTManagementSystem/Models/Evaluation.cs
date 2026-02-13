using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OJTManagementSystem.Models
{
    public class Evaluation
    {
        [Key]
        public int EvaluationId { get; set; }

        [Required]
        public int InternId { get; set; }

        [Required]
        public int SupervisorId { get; set; }

        [Range(1, 5)]
        public decimal TechnicalCompetence { get; set; }

        [Range(1, 5)]
        public decimal Punctuality { get; set; }

        [Range(1, 5)]
        public decimal Cooperation { get; set; }

        [Range(1, 5)]
        public decimal Communication { get; set; }

        [Range(1, 5)]
        public decimal QualityOfWork { get; set; }

        [Range(1, 5)]
        public decimal Reliability { get; set; }

        public decimal FinalRating { get; set; }

        [StringLength(1000)]
        public string Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(InternId))]
        public virtual Intern Intern { get; set; }

        [ForeignKey(nameof(SupervisorId))]
        public virtual Supervisor Supervisor { get; set; }
    }
}