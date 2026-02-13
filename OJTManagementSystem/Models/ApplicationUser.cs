using Microsoft.AspNetCore.Identity;

namespace OJTManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Intern> InternProfiles { get; set; } = new List<Intern>();
        public virtual ICollection<Supervisor> SupervisorProfiles { get; set; } = new List<Supervisor>();

        // ═══════════════════════════════════════════════════════════
        // ✅ CORRECTED: Only SentMessages (no ReceivedMessages)
        // ═══════════════════════════════════════════════════════════
        public virtual ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();

        // ❌ REMOVED: ReceivedMessages
        // ChatMessage doesn't have ReceiverId, so this relationship can't exist
        // Received messages are found by querying Conversations where user is User1 or User2
    }
}