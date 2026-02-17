using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OJTManagementSystem.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        // ✅ NEW: Read receipt fields
        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        // ═══════════════════════════════════════════════════════════
        // NAVIGATION PROPERTIES
        // ═══════════════════════════════════════════════════════════

        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual ApplicationUser Sender { get; set; }

        // ═══════════════════════════════════════════════════════════
        // IMPORTANT: We removed ReceiverId and Receiver properties
        // The receiver is determined from the Conversation relationship
        // If SenderId == Conversation.User1Id, then Receiver is User2
        // If SenderId == Conversation.User2Id, then Receiver is User1
        // ═══════════════════════════════════════════════════════════
    }
}