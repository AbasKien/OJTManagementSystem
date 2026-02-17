using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation?> GetPrivateConversationAsync(string user1Id, string user2Id)
        {
            return await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == user1Id && c.User2Id == user2Id) ||
                    (c.User1Id == user2Id && c.User2Id == user1Id));
        }

        public async Task CreateConversationAsync(Conversation conversation)
        {
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(string userId)
        {
            return await _context.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ NEW: READ RECEIPT METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mark all messages in a conversation as read by a specific user
        /// </summary>
        public async Task MarkMessagesAsReadAsync(int conversationId, string userId)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return;

            // Get all unread messages in this conversation that were NOT sent by this user
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId
                    && m.SenderId != userId
                    && !m.IsRead)
                .ToListAsync();

            // Mark them as read
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.Now;
            }

            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get total count of unread messages for a user across all conversations
        /// </summary>
        public async Task<int> GetUnreadMessageCountAsync(string userId)
        {
            var conversations = await _context.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Select(c => c.Id)
                .ToListAsync();

            return await _context.ChatMessages
                .Where(m => conversations.Contains(m.ConversationId)
                    && m.SenderId != userId
                    && !m.IsRead)
                .CountAsync();
        }

        /// <summary>
        /// Get unread message count for a specific conversation
        /// </summary>
        public async Task<int> GetUnreadMessageCountForConversationAsync(int conversationId, string userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId
                    && m.SenderId != userId
                    && !m.IsRead)
                .CountAsync();
        }
    }
}