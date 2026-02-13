namespace OJTManagementSystem.ViewModel
{
    /// <summary>
    /// ViewModel for displaying a list of messages in the Messages view
    /// </summary>
    public class MessageListViewModel
    {
        /// <summary>
        /// Collection of messages to display
        /// </summary>
        public List<MessageViewModel> Messages { get; set; } = new();

        /// <summary>
        /// Count of unread messages
        /// </summary>
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// ViewModel for individual message display
    /// </summary>
    public class MessageViewModel
    {
        public int Id { get; set; }

        public string SenderId { get; set; }
        public string SenderName { get; set; }

        public string ReceiverId { get; set; }
        public string ReceiverName { get; set; }

        public string Subject { get; set; }
        public string Content { get; set; }

        public DateTime DateSent { get; set; }

        public bool IsRead { get; set; }

        /// <summary>
        /// Whether this message was sent by the current user
        /// </summary>
        public bool IsSentByMe { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}