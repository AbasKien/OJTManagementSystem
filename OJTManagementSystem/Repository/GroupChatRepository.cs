using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    /// <summary>
    /// Repository implementation for GroupChat data access operations
    /// UPDATED with read receipt functionality
    /// </summary>
    public class GroupChatRepository : GenericRepository<GroupChat>, IGroupChatRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupChatRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all group chats for a specific user (where user is a member)
        /// </summary>
        public async Task<List<GroupChat>> GetUserGroupChatsAsync(string userId)
        {
            return await _context.GroupChats
                .Include(g => g.Creator)
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .Include(g => g.Messages)
                .Where(g => g.Members.Any(m => m.UserId == userId) && g.IsActive)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific group chat with all its members and messages
        /// </summary>
        public async Task<GroupChat> GetGroupChatWithDetailsAsync(int groupChatId)
        {
            return await _context.GroupChats
                .Include(g => g.Creator)
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .Include(g => g.Messages)
                .ThenInclude(m => m.Sender)
                .Include(g => g.Messages)
                .ThenInclude(m => m.ReadReceipts)  // ✅ NEW: Include read receipts
                .Where(g => g.GroupChatId == groupChatId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get recent messages from a group chat
        /// </summary>
        public async Task<List<GroupChatMessage>> GetGroupChatMessagesAsync(int groupChatId, int count = 50)
        {
            return await _context.GroupChatMessages
                .Where(m => m.GroupChatId == groupChatId)
                .Include(m => m.Sender)
                .Include(m => m.ReadReceipts)  // ✅ NEW: Include read receipts
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Add a member to a group chat
        /// </summary>
        public async Task<GroupChatMember> AddMemberAsync(int groupChatId, string userId, bool isAdmin = false)
        {
            var member = new GroupChatMember
            {
                GroupChatId = groupChatId,
                UserId = userId,
                IsAdmin = isAdmin,
                JoinedAt = DateTime.UtcNow,
                LastReadAt = DateTime.UtcNow  // ✅ NEW: Set initial read time
            };

            _context.GroupChatMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        /// <summary>
        /// Remove a member from a group chat
        /// </summary>
        public async Task<bool> RemoveMemberAsync(int groupChatId, string userId)
        {
            var member = await _context.GroupChatMembers
                .FirstOrDefaultAsync(m => m.GroupChatId == groupChatId && m.UserId == userId);

            if (member == null)
                return false;

            _context.GroupChatMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Check if a user is a member of a group chat
        /// </summary>
        public async Task<bool> IsMemberAsync(int groupChatId, string userId)
        {
            return await _context.GroupChatMembers
                .AnyAsync(m => m.GroupChatId == groupChatId && m.UserId == userId);
        }

        /// <summary>
        /// Add a message to a group chat
        /// </summary>
        public async Task<GroupChatMessage> AddMessageAsync(int groupChatId, string senderId, string messageContent)
        {
            var message = new GroupChatMessage
            {
                GroupChatId = groupChatId,
                SenderId = senderId,
                MessageContent = messageContent,
                CreatedAt = DateTime.UtcNow
            };

            _context.GroupChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // ✅ NEW: Automatically mark as read by sender
            await AddReadReceiptAsync(message.GroupChatMessageId, senderId);

            return message;
        }

        /// <summary>
        /// Get all members of a group chat
        /// </summary>
        public async Task<List<GroupChatMember>> GetMembersAsync(int groupChatId)
        {
            return await _context.GroupChatMembers
                .Where(m => m.GroupChatId == groupChatId)
                .Include(m => m.User)
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ NEW: READ RECEIPT METHODS FOR GROUP CHAT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mark all messages in a group chat as read by updating LastReadAt
        /// </summary>
        public async Task MarkGroupChatAsReadAsync(int groupChatId, string userId)
        {
            var member = await _context.GroupChatMembers
                .FirstOrDefaultAsync(m => m.GroupChatId == groupChatId && m.UserId == userId);

            if (member == null)
                return;

            member.LastReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Also add read receipts for all unread messages
            var messages = await _context.GroupChatMessages
                .Where(m => m.GroupChatId == groupChatId
                    && m.SenderId != userId
                    && m.CreatedAt > (member.LastReadAt ?? DateTime.MinValue))
                .ToListAsync();

            foreach (var message in messages)
            {
                var hasReceipt = await _context.GroupChatMessageReadReceipts
                    .AnyAsync(r => r.GroupChatMessageId == message.GroupChatMessageId && r.UserId == userId);

                if (!hasReceipt)
                {
                    _context.GroupChatMessageReadReceipts.Add(new GroupChatMessageReadReceipt
                    {
                        GroupChatMessageId = message.GroupChatMessageId,
                        UserId = userId,
                        ReadAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get total unread message count across all group chats for a user
        /// </summary>
        public async Task<int> GetUnreadGroupMessageCountAsync(string userId)
        {
            var member = await _context.GroupChatMembers
                .Where(m => m.UserId == userId)
                .ToListAsync();

            int totalUnread = 0;

            foreach (var m in member)
            {
                var unreadCount = await _context.GroupChatMessages
                    .Where(msg => msg.GroupChatId == m.GroupChatId
                        && msg.SenderId != userId
                        && msg.CreatedAt > (m.LastReadAt ?? DateTime.MinValue))
                    .CountAsync();

                totalUnread += unreadCount;
            }

            return totalUnread;
        }

        /// <summary>
        /// Get unread message count for a specific group chat
        /// </summary>
        public async Task<int> GetUnreadGroupMessageCountForChatAsync(int groupChatId, string userId)
        {
            var member = await _context.GroupChatMembers
                .FirstOrDefaultAsync(m => m.GroupChatId == groupChatId && m.UserId == userId);

            if (member == null)
                return 0;

            return await _context.GroupChatMessages
                .Where(m => m.GroupChatId == groupChatId
                    && m.SenderId != userId
                    && m.CreatedAt > (member.LastReadAt ?? DateTime.MinValue))
                .CountAsync();
        }

        /// <summary>
        /// Add a read receipt for a specific message
        /// </summary>
        public async Task AddReadReceiptAsync(int messageId, string userId)
        {
            var exists = await _context.GroupChatMessageReadReceipts
                .AnyAsync(r => r.GroupChatMessageId == messageId && r.UserId == userId);

            if (exists)
                return;

            var receipt = new GroupChatMessageReadReceipt
            {
                GroupChatMessageId = messageId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            };

            _context.GroupChatMessageReadReceipts.Add(receipt);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if a user has read a specific message
        /// </summary>
        public async Task<bool> HasUserReadMessageAsync(int messageId, string userId)
        {
            return await _context.GroupChatMessageReadReceipts
                .AnyAsync(r => r.GroupChatMessageId == messageId && r.UserId == userId);
        }

        /// <summary>
        /// Get how many people have read a message
        /// </summary>
        public async Task<int> GetMessageReadCountAsync(int messageId)
        {
            return await _context.GroupChatMessageReadReceipts
                .Where(r => r.GroupChatMessageId == messageId)
                .CountAsync();
        }
    }
}