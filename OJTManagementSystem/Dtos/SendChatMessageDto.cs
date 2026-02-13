using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.Dtos
{
    public class SendChatMessageDto
    {
        [Required(ErrorMessage = "Receiver is required")]
        public string ReceiverId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, MinimumLength = 1)]
        public string MessageContent { get; set; }
    }
}