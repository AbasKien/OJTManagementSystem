using Microsoft.AspNetCore.Mvc;

namespace OJTManagementSystem.Helpers
{
    /// <summary>
    /// Simple notification helper using TempData
    /// </summary>
    public static class NotificationHelper
    {
        // ============================================================
        // SIMPLE NOTIFICATION METHODS
        // ============================================================

        public static void ShowSuccess(Controller controller, string message)
        {
            controller.TempData["Success"] = message;
        }

        public static void ShowError(Controller controller, string message)
        {
            controller.TempData["Error"] = message;
        }

        public static void ShowInfo(Controller controller, string message)
        {
            controller.TempData["Info"] = message;
        }

        public static void ShowWarning(Controller controller, string message)
        {
            controller.TempData["Warning"] = message;
        }

        /// <summary>
        /// Show an info notification with a clickable link button.
        /// </summary>
        public static void ShowInfoWithLink(Controller controller, string message, string linkUrl, string linkText)
        {
            controller.TempData["Info"] = message;
            controller.TempData["InfoLinkUrl"] = linkUrl;
            controller.TempData["InfoLinkText"] = linkText;
        }

        // ============================================================
        // DTR NOTIFICATIONS
        // ============================================================

        public static void NotifyDtrSubmitted(Controller controller)
        {
            ShowSuccess(controller, "DTR submitted successfully! Your supervisor will review it soon.");
        }

        public static void NotifyDtrApproved(Controller controller, string date)
        {
            ShowSuccess(controller, $"DTR for {date} has been approved!");
        }

        public static void NotifyDtrRejected(Controller controller, string date, string reason)
        {
            ShowWarning(controller, $"DTR for {date} was rejected. Reason: {reason}");
        }

        // ============================================================
        // LEAVE REQUEST NOTIFICATIONS
        // ============================================================

        public static void NotifyLeaveSubmitted(Controller controller)
        {
            ShowSuccess(controller, "Leave request submitted successfully! Waiting for supervisor approval.");
        }

        public static void NotifyLeaveApproved(Controller controller, string dateRange)
        {
            ShowSuccess(controller, $"Your leave request for {dateRange} has been approved!");
        }

        public static void NotifyLeaveRejected(Controller controller, string dateRange, string reason)
        {
            ShowWarning(controller, $"Leave request for {dateRange} was rejected. Reason: {reason}");
        }

        // ============================================================
        // EVALUATION NOTIFICATIONS
        // ============================================================

        public static void NotifyEvaluationReceived(Controller controller, string evaluationType)
        {
            ShowInfo(controller, $"You have received a new {evaluationType} evaluation from your supervisor.");
        }

        public static void NotifyEvaluationSubmitted(Controller controller, string internName)
        {
            ShowSuccess(controller, $"Evaluation for {internName} submitted successfully!");
        }

        // ============================================================
        // CERTIFICATE NOTIFICATIONS
        // ============================================================

        public static void NotifyCertificateGenerated(Controller controller)
        {
            ShowSuccess(controller, "Your OJT certificate has been generated and is ready for download!");
        }

        // ============================================================
        // MESSAGE NOTIFICATIONS (PRIVATE CHAT)
        // ============================================================

        /// <summary>
        /// Generic notification when message is sent (private chat)
        /// </summary>
        public static void NotifyMessageSent(Controller controller)
        {
            ShowSuccess(controller, "Message sent successfully!");
        }

        /// <summary>
        /// Notification with recipient name (private chat)
        /// </summary>
        public static void NotifyPrivateMessageSent(Controller controller, string recipientName)
        {
            ShowSuccess(controller, $"Message sent to {recipientName}!");
        }

        /// <summary>
        /// Notify about new messages since last visit, with a link to AllChats
        /// </summary>
        public static void NotifyNewMessages(Controller controller, int count)
        {
            ShowInfoWithLink(
                controller,
                message: $"You have {count} new message{(count > 1 ? "s" : "")} since your last visit.",
                linkUrl: "/Intern/AllChats",
                linkText: "View Messages"
            );
        }

        // ============================================================
        // GROUP CHAT NOTIFICATIONS ✅ NEW!
        // ============================================================

        /// <summary>
        /// Notification when group message is sent
        /// </summary>
        public static void NotifyGroupMessageSent(Controller controller, string groupName)
        {
            ShowSuccess(controller, $"Message sent to {groupName}!");
        }

        /// <summary>
        /// Generic notification for group message (when group name not available)
        /// </summary>
        public static void NotifyGroupMessageSentGeneric(Controller controller)
        {
            ShowSuccess(controller, "Group message sent successfully!");
        }

        /// <summary>
        /// Notification when group chat is created
        /// </summary>
        public static void NotifyGroupChatCreated(Controller controller, string groupName, int memberCount)
        {
            ShowSuccess(controller, $"Group chat '{groupName}' created with {memberCount} member{(memberCount > 1 ? "s" : "")}!");
        }

        /// <summary>
        /// Notification when member is added to group
        /// </summary>
        public static void NotifyMemberAdded(Controller controller, string memberName, string groupName)
        {
            ShowSuccess(controller, $"{memberName} added to {groupName}!");
        }

        /// <summary>
        /// Notification when member is removed from group
        /// </summary>
        public static void NotifyMemberRemoved(Controller controller, string memberName, string groupName)
        {
            ShowInfo(controller, $"{memberName} removed from {groupName}.");
        }

        /// <summary>
        /// Notification when group chat is deleted
        /// </summary>
        public static void NotifyGroupChatDeleted(Controller controller, string groupName)
        {
            ShowInfo(controller, $"Group chat '{groupName}' deleted successfully.");
        }

        /// <summary>
        /// Notification when you're added to a group
        /// </summary>
        public static void NotifyAddedToGroup(Controller controller, string groupName)
        {
            ShowInfoWithLink(
                controller,
                message: $"You've been added to '{groupName}'!",
                linkUrl: "/Intern/AllChats",
                linkText: "View Group"
            );
        }
    }
}