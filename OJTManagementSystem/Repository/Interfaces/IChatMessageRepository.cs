using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    /// <summary>
    /// Interface for chat message repository operations
    /// </summary>
    public interface IChatMessageRepository
    {
        Task<Conversation> GetPrivateConversationAsync(string user1Id, string user2Id);
        Task CreateConversationAsync(Conversation conversation);
        Task AddMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetMessagesByConversationIdAsync(int conversationId);
        Task<List<Conversation>> GetUserConversationsAsync(string userId);

        // ✅ Read receipt methods
        Task MarkMessagesAsReadAsync(int conversationId, string userId);
        Task<int> GetUnreadMessageCountAsync(string userId);
        Task<int> GetUnreadMessageCountForConversationAsync(int conversationId, string userId);
    }
}