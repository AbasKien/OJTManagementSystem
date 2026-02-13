using OJTManagementSystem.Models;

namespace OJTManagementSystem.Models
{
    public class Intern
    {
        public int InternId { get; set; }
        public string UserId { get; set; }

        public string StudentId { get; set; }
        public string School { get; set; }
        public string Course { get; set; }
        public string Department { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // ✅ CHANGED: SupervisorId is now nullable (intern doesn't select supervisor during registration)
        public int? SupervisorId { get; set; }

        public decimal CompletedHours { get; set; } = 0m;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual Supervisor Supervisor { get; set; }

        public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

        public virtual ICollection<DailyTimeRecord> DailyTimeRecords { get; set; } = new List<DailyTimeRecord>();
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    }
}