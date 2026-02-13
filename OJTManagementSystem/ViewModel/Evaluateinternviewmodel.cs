namespace OJTManagementSystem.ViewModel
{
    public class EvaluateInternViewModel
    {
        public int InternId { get; set; }

        // ✅ FIXED: Added proper initialization of ratings (default to 0)
        private decimal _technicalCompetence = 0;
        private decimal _punctuality = 0;
        private decimal _cooperation = 0;
        private decimal _communication = 0;
        private decimal _qualityOfWork = 0;
        private decimal _reliability = 0;

        public decimal TechnicalCompetence
        {
            get => _technicalCompetence;
            set => _technicalCompetence = value;
        }

        public decimal Punctuality
        {
            get => _punctuality;
            set => _punctuality = value;
        }

        public decimal Cooperation
        {
            get => _cooperation;
            set => _cooperation = value;
        }

        public decimal Communication
        {
            get => _communication;
            set => _communication = value;
        }

        public decimal QualityOfWork
        {
            get => _qualityOfWork;
            set => _qualityOfWork = value;
        }

        public decimal Reliability
        {
            get => _reliability;
            set => _reliability = value;
        }

        public string Comments { get; set; }
    }
}