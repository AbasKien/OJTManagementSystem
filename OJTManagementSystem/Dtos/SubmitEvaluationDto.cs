using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.Dtos
{
    public class SubmitEvaluationDto
    {
        [Required]
        public int InternId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal TechnicalCompetence { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal Punctuality { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal Cooperation { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal Communication { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal QualityOfWork { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal Reliability { get; set; }

        [StringLength(1000)]
        public string Comments { get; set; }
    }
}