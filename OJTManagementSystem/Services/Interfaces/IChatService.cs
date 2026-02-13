using OJTManagementSystem.Models;

namespace OJTManagementSystem.Services.Interfaces
{
    public interface IChatService
    {
        Task<Conversation> GetOrCreatePrivateConversationAsync(string user1Id, string user2Id);
        Task SendPrivateMessageAsync(string senderId, string receiverId, string content);
        Task<List<ChatMessage>> GetMessagesByConversationIdAsync(int conversationId);
        Task<List<Conversation>> GetUserConversationsAsync(string userId);
    }
}
