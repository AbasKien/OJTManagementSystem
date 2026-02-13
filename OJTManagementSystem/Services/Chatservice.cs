using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.Services.Interfaces;

namespace OJTManagementSystem.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatMessageRepository _chatRepository;

        public ChatService(IChatMessageRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<Conversation> GetOrCreatePrivateConversationAsync(string user1Id, string user2Id)
        {
            var conversation = await _chatRepository
                .GetPrivateConversationAsync(user1Id, user2Id);

            if (conversation != null)
                return conversation;

            var newConversation = new Conversation
            {
                User1Id = user1Id,
                User2Id = user2Id
            };

            await _chatRepository.CreateConversationAsync(newConversation);

            return newConversation;
        }

        public async Task SendPrivateMessageAsync(string senderId, string receiverId, string content)
        {
            var conversation = await GetOrCreatePrivateConversationAsync(senderId, receiverId);

            var message = new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderId = senderId,
                Content = content
            };

            await _chatRepository.AddMessageAsync(message);
        }

        public async Task<List<ChatMessage>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await _chatRepository.GetMessagesByConversationIdAsync(conversationId);
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(string userId)
        {
            return await _chatRepository.GetUserConversationsAsync(userId);
        }
    }
}
