using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    /// <summary>
    /// Repository implementation for GroupChat data access operations
    /// UPDATED to work with your existing IGenericRepository interface
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
                JoinedAt = DateTime.UtcNow
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
    }
}