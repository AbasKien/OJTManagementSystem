namespace OJTManagementSystem.Models
{
    public class Certificate
    {
        public int CertificateId { get; set; }
        public int InternId { get; set; }

        public string CertificateNumber { get; set; }

        public DateTime IssuedDate { get; set; }

        // ✅ FIXED: Changed from read-only to read-write property
        public DateTime IssueDate { get; set; }

        // ✅ ADDED: StartDate and EndDate properties (needed by service)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalHoursRendered { get; set; }

        // ✅ FIXED: Changed from read-only to read-write property
        public decimal TotalHoursCompleted { get; set; }

        // ✅ ADDED: PdfContent and PdfFileName properties (needed by service)
        public byte[] PdfContent { get; set; }
        public string PdfFileName { get; set; }

        public string IssuedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Intern Intern { get; set; }
    }
}