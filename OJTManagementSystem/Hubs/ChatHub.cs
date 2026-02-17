using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using OJTManagementSystem.Models;
using OJTManagementSystem.Services.Interfaces;
using OJTManagementSystem.Dtos;
using System.Linq;

namespace OJTManagementSystem.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IGroupChatService _groupChatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(
            IChatService chatService,
            IGroupChatService groupChatService,
            UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _groupChatService = groupChatService;
            _userManager = userManager;
        }

        // ═══════════════════════════════════════════════════════════
        // SEND MESSAGES
        // ═══════════════════════════════════════════════════════════

        public async Task SendPrivateMessage(string receiverId, string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);

            // Send message to database
            await _chatService.SendPrivateMessageAsync(user.Id, receiverId, message);

            // Get the conversation to get the message with its ID
            var conversation = await _chatService.GetOrCreatePrivateConversationAsync(user.Id, receiverId);
            var messages = await _chatService.GetMessagesByConversationIdAsync(conversation.Id);
            var latestMessage = messages.OrderByDescending(m => m.SentAt).First();

            // Send to receiver in real-time
            await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", new
            {
                messageId = latestMessage.Id,
                senderId = user.Id,
                senderName = user.FullName,
                content = message,
                sentAt = latestMessage.SentAt,
                isRead = false
            });

            // Send confirmation back to sender
            await Clients.Caller.SendAsync("MessageSent", new
            {
                messageId = latestMessage.Id,
                isRead = false
            });

            // Update badge count for receiver
            var privateUnread = await _chatService.GetUnreadMessageCountAsync(receiverId);
            var groupUnread = await _groupChatService.GetUnreadGroupMessageCountAsync(receiverId);
            await Clients.User(receiverId).SendAsync("UpdateUnreadBadge", privateUnread + groupUnread);
        }

        public async Task SendGroupMessage(int groupChatId, string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);

            var dto = new SendGroupChatMessageDto { MessageContent = message };
            var savedMessage = await _groupChatService.SendMessageAsync(groupChatId, user.Id, dto);

            var members = await _groupChatService.GetGroupChatMembersAsync(groupChatId);

            foreach (var member in members)
            {
                if (member.UserId != user.Id)
                {
                    await Clients.User(member.UserId).SendAsync("ReceiveGroupMessage", new
                    {
                        groupChatId = groupChatId,
                        messageId = savedMessage.GroupChatMessageId,
                        senderId = user.Id,
                        senderName = user.FullName,
                        content = message,
                        sentAt = savedMessage.CreatedAt
                    });

                    var privateUnread = await _chatService.GetUnreadMessageCountAsync(member.UserId);
                    var groupUnread = await _groupChatService.GetUnreadGroupMessageCountAsync(member.UserId);
                    await Clients.User(member.UserId).SendAsync("UpdateUnreadBadge", privateUnread + groupUnread);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ READ RECEIPTS
        // ═══════════════════════════════════════════════════════════

        public async Task MarkPrivateMessagesAsRead(int conversationId, string otherUserId)
        {
            var user = await _userManager.GetUserAsync(Context.User);

            // Mark messages as read in database
            await _chatService.MarkMessagesAsReadAsync(conversationId, user.Id);

            // Get all messages that were just marked as read
            var messages = await _chatService.GetMessagesByConversationIdAsync(conversationId);
            var readMessages = messages
                .Where(m => m.SenderId == otherUserId && m.IsRead)
                .Select(m => m.Id)
                .ToList();

            // Notify the sender that their messages were read (double check!)
            await Clients.User(otherUserId).SendAsync("MessagesRead", new
            {
                messageIds = readMessages,
                readBy = user.Id,
                readByName = user.FullName,
                readAt = DateTime.UtcNow
            });

            // Update badge count for current user
            var privateUnread = await _chatService.GetUnreadMessageCountAsync(user.Id);
            var groupUnread = await _groupChatService.GetUnreadGroupMessageCountAsync(user.Id);
            await Clients.Caller.SendAsync("UpdateUnreadBadge", privateUnread + groupUnread);
        }

        public async Task MarkGroupMessagesAsRead(int groupChatId)
        {
            var user = await _userManager.GetUserAsync(Context.User);

            await _groupChatService.MarkGroupChatAsReadAsync(groupChatId, user.Id);

            var members = await _groupChatService.GetGroupChatMembersAsync(groupChatId);

            foreach (var member in members)
            {
                if (member.UserId != user.Id)
                {
                    await Clients.User(member.UserId).SendAsync("GroupMessagesRead", new
                    {
                        groupChatId = groupChatId,
                        readBy = user.Id,
                        readByName = user.FullName,
                        readAt = DateTime.UtcNow
                    });
                }
            }

            var privateUnread = await _chatService.GetUnreadMessageCountAsync(user.Id);
            var groupUnread = await _groupChatService.GetUnreadGroupMessageCountAsync(user.Id);
            await Clients.Caller.SendAsync("UpdateUnreadBadge", privateUnread + groupUnread);
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                var privateUnread = await _chatService.GetUnreadMessageCountAsync(user.Id);
                var groupUnread = await _groupChatService.GetUnreadGroupMessageCountAsync(user.Id);
                await Clients.Caller.SendAsync("UpdateUnreadBadge", privateUnread + groupUnread);
            }

            await base.OnConnectedAsync();
        }
    }
}