using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.Dtos
{
    /// <summary>
    /// DTO for creating a new group chat
    /// </summary>
    public class CreateGroupChatDto
    {
        [Required(ErrorMessage = "Group name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Group name must be between 3 and 200 characters")]
        public string GroupName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO for sending a message in a group chat
    /// </summary>
    public class SendGroupChatMessageDto
    {
        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 1000 characters")]
        public string MessageContent { get; set; }
    }

    /// <summary>
    /// DTO for adding a member to a group chat
    /// </summary>
    public class AddGroupChatMemberDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }
    }
}