using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    /// <summary>
    /// Interface for group chat repository operations
    /// Works with your existing IGenericRepository<T> interface
    /// </summary>
    public interface IGroupChatRepository : IGenericRepository<GroupChat>
    {
        Task<List<GroupChat>> GetUserGroupChatsAsync(string userId);
        Task<GroupChat> GetGroupChatWithDetailsAsync(int groupChatId);
        Task<List<GroupChatMessage>> GetGroupChatMessagesAsync(int groupChatId, int count = 50);
        Task<GroupChatMember> AddMemberAsync(int groupChatId, string userId, bool isAdmin = false);
        Task<bool> RemoveMemberAsync(int groupChatId, string userId);
        Task<bool> IsMemberAsync(int groupChatId, string userId);
        Task<GroupChatMessage> AddMessageAsync(int groupChatId, string senderId, string messageContent);
        Task<List<GroupChatMember>> GetMembersAsync(int groupChatId);
    }
}