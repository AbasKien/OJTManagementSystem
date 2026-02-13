using OJTManagementSystem.Dtos;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.ViewModel;
using OJTManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace OJTManagementSystem.Services
{
    /// <summary>
    /// Service for group chat operations
    /// UPDATED to work with your existing IGenericRepository interface
    /// </summary>
    public class GroupChatService : IGroupChatService
    {
        private readonly IGroupChatRepository _groupChatRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupChatService(
            IGroupChatRepository groupChatRepository,
            UserManager<ApplicationUser> userManager)
        {
            _groupChatRepository = groupChatRepository;
            _userManager = userManager;
        }

        /// <summary>
        /// Create a new group chat
        /// </summary>
        public async Task<GroupChatViewModel> CreateGroupChatAsync(string creatorId, CreateGroupChatDto dto)
        {
            var creator = await _userManager.FindByIdAsync(creatorId);
            if (creator == null)
                throw new Exception("Creator not found");

            var groupChat = new GroupChat
            {
                GroupName = dto.GroupName,
                Description = dto.Description,
                CreatedBy = creatorId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Use AddAsync from IGenericRepository
            await _groupChatRepository.AddAsync(groupChat);

            // Add creator as admin member
            await _groupChatRepository.AddMemberAsync(groupChat.GroupChatId, creatorId, isAdmin: true);

            return await GetGroupChatByIdAsync(groupChat.GroupChatId);
        }

        /// <summary>
        /// Get all group chats for a user
        /// </summary>
        public async Task<List<GroupChatViewModel>> GetUserGroupChatsAsync(string userId)
        {
            var groupChats = await _groupChatRepository.GetUserGroupChatsAsync(userId);
            return groupChats.Select(MapToViewModel).ToList();
        }

        /// <summary>
        /// Get a specific group chat by ID
        /// </summary>
        public async Task<GroupChatViewModel> GetGroupChatByIdAsync(int groupChatId)
        {
            var groupChat = await _groupChatRepository.GetGroupChatWithDetailsAsync(groupChatId);
            if (groupChat == null)
                throw new Exception("Group chat not found");

            return MapToViewModel(groupChat);
        }

        /// <summary>
        /// Add a member to a group chat
        /// </summary>
        public async Task<bool> AddMemberAsync(int groupChatId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var isMember = await _groupChatRepository.IsMemberAsync(groupChatId, userId);
            if (isMember)
                throw new Exception("User is already a member of this group");

            await _groupChatRepository.AddMemberAsync(groupChatId, userId, isAdmin: false);
            return true;
        }

        /// <summary>
        /// Remove a member from a group chat
        /// </summary>
        public async Task<bool> RemoveMemberAsync(int groupChatId, string userId)
        {
            return await _groupChatRepository.RemoveMemberAsync(groupChatId, userId);
        }

        /// <summary>
        /// Send a message in a group chat
        /// </summary>
        public async Task<GroupChatMessageViewModel> SendMessageAsync(int groupChatId, string senderId, SendGroupChatMessageDto dto)
        {
            var sender = await _userManager.FindByIdAsync(senderId);
            if (sender == null)
                throw new Exception("Sender not found");

            var isMember = await _groupChatRepository.IsMemberAsync(groupChatId, senderId);
            if (!isMember)
                throw new Exception("You are not a member of this group chat");

            var message = await _groupChatRepository.AddMessageAsync(groupChatId, senderId, dto.MessageContent);

            return MapMessageToViewModel(message, sender);
        }

        /// <summary>
        /// Get messages from a group chat
        /// </summary>
        public async Task<List<GroupChatMessageViewModel>> GetGroupChatMessagesAsync(int groupChatId)
        {
            var messages = await _groupChatRepository.GetGroupChatMessagesAsync(groupChatId);
            return messages.Select(m => MapMessageToViewModel(m, m.Sender)).ToList();
        }

        /// <summary>
        /// Get members of a group chat
        /// </summary>
        public async Task<List<GroupChatMemberViewModel>> GetGroupChatMembersAsync(int groupChatId)
        {
            var members = await _groupChatRepository.GetMembersAsync(groupChatId);
            return members.Select(m => new GroupChatMemberViewModel
            {
                GroupChatMemberId = m.GroupChatMemberId,
                UserId = m.UserId,
                UserName = m.User?.FullName,
                IsAdmin = m.IsAdmin,
                JoinedAt = m.JoinedAt
            }).ToList();
        }

        /// <summary>
        /// Check if user is member of group chat
        /// </summary>
        public async Task<bool> IsMemberAsync(int groupChatId, string userId)
        {
            return await _groupChatRepository.IsMemberAsync(groupChatId, userId);
        }

        /// <summary>
        /// Delete a group chat (only creator/admin can do this)
        /// </summary>
        public async Task<bool> DeleteGroupChatAsync(int groupChatId, string userId)
        {
            // Use GetByIdAsync from IGenericRepository
            var groupChat = await _groupChatRepository.GetByIdAsync(groupChatId);
            if (groupChat == null)
                throw new Exception("Group chat not found");

            // Only creator can delete
            if (groupChat.CreatedBy != userId)
                throw new Exception("Only the creator can delete this group chat");

            groupChat.IsActive = false;

            // Use UpdateAsync from IGenericRepository
            await _groupChatRepository.UpdateAsync(groupChat);
            return true;
        }

        private GroupChatViewModel MapToViewModel(GroupChat groupChat)
        {
            return new GroupChatViewModel
            {
                GroupChatId = groupChat.GroupChatId,
                GroupName = groupChat.GroupName,
                Description = groupChat.Description,
                CreatorId = groupChat.CreatedBy,
                CreatorName = groupChat.Creator?.FullName,
                CreatedAt = groupChat.CreatedAt,
                MemberCount = groupChat.Members?.Count ?? 0,
                MessageCount = groupChat.Messages?.Count ?? 0,
                Members = groupChat.Members?.Select(m => new GroupChatMemberViewModel
                {
                    GroupChatMemberId = m.GroupChatMemberId,
                    UserId = m.UserId,
                    UserName = m.User?.FullName,
                    IsAdmin = m.IsAdmin,
                    JoinedAt = m.JoinedAt
                }).ToList() ?? new List<GroupChatMemberViewModel>(),
                Messages = groupChat.Messages?.Select(m => MapMessageToViewModel(m, m.Sender)).ToList() ?? new List<GroupChatMessageViewModel>()
            };
        }

        private GroupChatMessageViewModel MapMessageToViewModel(GroupChatMessage message, ApplicationUser sender)
        {
            return new GroupChatMessageViewModel
            {
                GroupChatMessageId = message.GroupChatMessageId,
                GroupChatId = message.GroupChatId,
                SenderId = message.SenderId,
                SenderName = sender?.FullName,
                MessageContent = message.MessageContent,
                CreatedAt = message.CreatedAt
            };
        }
    }
}