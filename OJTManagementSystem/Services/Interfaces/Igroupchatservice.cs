using OJTManagementSystem.Dtos;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for group chat service operations
    /// </summary>
    public interface IGroupChatService
    {
        Task<GroupChatViewModel> CreateGroupChatAsync(string creatorId, CreateGroupChatDto dto);
        Task<List<GroupChatViewModel>> GetUserGroupChatsAsync(string userId);
        Task<GroupChatViewModel> GetGroupChatByIdAsync(int groupChatId);
        Task<bool> AddMemberAsync(int groupChatId, string userId);
        Task<bool> RemoveMemberAsync(int groupChatId, string userId);
        Task<GroupChatMessageViewModel> SendMessageAsync(int groupChatId, string senderId, SendGroupChatMessageDto dto);
        Task<List<GroupChatMessageViewModel>> GetGroupChatMessagesAsync(int groupChatId);
        Task<List<GroupChatMemberViewModel>> GetGroupChatMembersAsync(int groupChatId);
        Task<bool> IsMemberAsync(int groupChatId, string userId);
        Task<bool> DeleteGroupChatAsync(int groupChatId, string userId);
    }
}