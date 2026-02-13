namespace OJTManagementSystem.ViewModel
{
    public class SupervisorDashboardViewModel
    {
        // ═══════════════════════════════════════════════════════════
        // EXISTING PROPERTIES
        // ═══════════════════════════════════════════════════════════
        public int InternCount { get; set; }
        public int PendingDtrCount { get; set; }
        public int PendingLeaveRequestCount { get; set; }
        public int PendingEvaluationCount { get; set; }
        public List<DailyTimeRecordViewModel> PendingDtrs { get; set; } = new List<DailyTimeRecordViewModel>();
        public List<LeaveRequestViewModel> PendingLeaveRequests { get; set; } = new List<LeaveRequestViewModel>();
        public List<InternViewModel> Interns { get; set; } = new List<InternViewModel>();

        // ═══════════════════════════════════════════════════════════
        // ✅ NEW PROPERTIES FOR MESSAGING
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Count of unread messages
        /// </summary>
        public int UnreadMessageCount { get; set; }

        /// <summary>
        /// List of recent group chats
        /// </summary>
        public List<GroupChatViewModel> RecentGroupChats { get; set; } = new List<GroupChatViewModel>();

        /// <summary>
        /// List of recent conversations
        /// </summary>
        public List<PrivateChatViewModel> RecentPrivateChats { get; set; } = new List<PrivateChatViewModel>();
    }
}