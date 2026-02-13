namespace OJTManagementSystem.ViewModel
{
    public class EvaluationViewModel
    {
        public int EvaluationId { get; set; }
        public int InternId { get; set; }
        public string InternName { get; set; }
        public int SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public decimal TechnicalCompetence { get; set; }
        public decimal Punctuality { get; set; }
        public decimal Cooperation { get; set; }
        public decimal Communication { get; set; }
        public decimal QualityOfWork { get; set; }
        public decimal Reliability { get; set; }
        public decimal FinalRating { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}