using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.ViewModel
{
    /// <summary>
    /// ViewModel for displaying a group chat
    /// </summary>
    public class GroupChatViewModel
    {
        public int GroupChatId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public int MessageCount { get; set; }

        // ✅ NEW: Unread message count
        public int UnreadMessageCount { get; set; }

        public List<GroupChatMemberViewModel> Members { get; set; } = new();
        public List<GroupChatMessageViewModel> Messages { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for creating a new group chat
    /// </summary>
    public class CreateGroupChatViewModel
    {
        [Required(ErrorMessage = "Group name is required")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters")]
        public string GroupName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }
    }

    /// <summary>
    /// ViewModel for a member in a group chat
    /// </summary>
    public class GroupChatMemberViewModel
    {
        public int GroupChatMemberId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }

        // ✅ NEW: Last read timestamp
        public DateTime? LastReadAt { get; set; }
    }

    /// <summary>
    /// ViewModel for a message in a group chat
    /// ✅ UPDATED with read receipt info
    /// </summary>
    public class GroupChatMessageViewModel
    {
        public int GroupChatMessageId { get; set; }
        public int GroupChatId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string MessageContent { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ NEW: Read receipt information
        public int ReadCount { get; set; }
        public int TotalMembers { get; set; }
        public bool IsReadByCurrentUser { get; set; }
        public List<string> ReadByUserNames { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for private chat conversation
    /// </summary>
    public class PrivateChatViewModel
    {
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserRole { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new();

        // ✅ NEW: Unread count for this conversation
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// ViewModel for listing all chats (both private and group)
    /// </summary>
    public class AllChatsViewModel
    {
        public List<GroupChatViewModel> GroupChats { get; set; } = new();
        public List<PrivateChatViewModel> PrivateChats { get; set; } = new();
        public int UnreadMessageCount { get; set; }

        // ✅ NEW: Separate counts
        public int UnreadPrivateCount { get; set; }
        public int UnreadGroupCount { get; set; }
    }
}