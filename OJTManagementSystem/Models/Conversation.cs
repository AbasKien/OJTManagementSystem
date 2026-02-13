using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        [Required]
        public string User1Id { get; set; }

        [Required]
        public string User2Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ChatMessage> Messages { get; set; }
    }
}
