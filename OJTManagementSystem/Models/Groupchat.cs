using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OJTManagementSystem.Models
{
    /// <summary>
    /// Represents a group chat (team chat) for multiple users
    /// </summary>
    public class GroupChat
    {
        [Key]
        public int GroupChatId { get; set; }

        [Required]
        [StringLength(200)]
        public string GroupName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(CreatedBy))]
        public virtual ApplicationUser Creator { get; set; }

        public virtual ICollection<GroupChatMember> Members { get; set; } = new List<GroupChatMember>();

        public virtual ICollection<GroupChatMessage> Messages { get; set; } = new List<GroupChatMessage>();
    }

    /// <summary>
    /// Represents a member of a group chat
    /// </summary>
    public class GroupChatMember
    {
        [Key]
        public int GroupChatMemberId { get; set; }

        [Required]
        public int GroupChatId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsAdmin { get; set; } = false;

        // Navigation properties
        [ForeignKey(nameof(GroupChatId))]
        public virtual GroupChat GroupChat { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }

    /// <summary>
    /// Represents a message in a group chat
    /// </summary>
    public class GroupChatMessage
    {
        [Key]
        public int GroupChatMessageId { get; set; }

        [Required]
        public int GroupChatId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        [StringLength(1000)]
        public string MessageContent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(GroupChatId))]
        public virtual GroupChat GroupChat { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual ApplicationUser Sender { get; set; }
    }
}