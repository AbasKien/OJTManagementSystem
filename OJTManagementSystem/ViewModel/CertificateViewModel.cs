namespace OJTManagementSystem.ViewModel
{
    public class CertificateViewModel
    {
        public int CertificateId { get; set; }
        public int InternId { get; set; }
        public string InternName { get; set; }
        public string InternFullName => InternName;
        public string School { get; set; }
        public string Course { get; set; }
        public string CertificateNumber { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime IssueDate => IssuedDate;
        public decimal TotalHoursRendered { get; set; }
        public decimal TotalHoursCompleted => TotalHoursRendered;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IssuedBy { get; set; }
        public string PdfFileName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}