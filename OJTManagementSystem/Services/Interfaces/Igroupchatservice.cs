using OJTManagementSystem.Dtos;
using OJTManagementSystem.Models;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for group chat service operations
    /// Matches the actual GroupChatService implementation
    /// </summary>
    public interface IGroupChatService
    {
        // Basic CRUD operations
        Task<GroupChatViewModel> CreateGroupChatAsync(string creatorId, CreateGroupChatDto dto);
        Task<List<GroupChatViewModel>> GetUserGroupChatsAsync(string userId);
        Task<GroupChatViewModel> GetGroupChatByIdAsync(int groupChatId);
        Task<bool> DeleteGroupChatAsync(int groupChatId, string userId);

        // Message operations
        Task<GroupChatMessageViewModel> SendMessageAsync(int groupChatId, string senderId, SendGroupChatMessageDto dto);
        Task<List<GroupChatMessageViewModel>> GetGroupChatMessagesAsync(int groupChatId);

        // Member operations
        Task<bool> AddMemberAsync(int groupChatId, string userId);
        Task<bool> RemoveMemberAsync(int groupChatId, string userId);
        Task<bool> IsMemberAsync(int groupChatId, string userId);
        Task<List<GroupChatMemberViewModel>> GetGroupChatMembersAsync(int groupChatId);

        // Read receipt methods (delegate to repository)
        Task MarkGroupChatAsReadAsync(int groupChatId, string userId);
        Task<int> GetUnreadGroupMessageCountAsync(string userId);
        Task<int> GetUnreadGroupMessageCountForChatAsync(int groupChatId, string userId);
        Task AddReadReceiptAsync(int messageId, string userId);
        Task<bool> HasUserReadMessageAsync(int messageId, string userId);
        Task<int> GetMessageReadCountAsync(int messageId);
    }
}