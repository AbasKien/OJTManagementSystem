using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    public interface IChatMessageRepository
    {
        Task<Conversation?> GetPrivateConversationAsync(string user1Id, string user2Id);
        Task CreateConversationAsync(Conversation conversation);
        Task AddMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetMessagesByConversationIdAsync(int conversationId);
        Task<List<Conversation>> GetUserConversationsAsync(string userId);
    }
}
