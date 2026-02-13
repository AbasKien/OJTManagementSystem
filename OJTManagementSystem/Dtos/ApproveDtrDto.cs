using System.ComponentModel.DataAnnotations;
using OJTManagementSystem.Enums;

namespace OJTManagementSystem.Dtos
{
    public class ApproveDtrDto
    {
        [Required]
        public int DtrId { get; set; }

        [Required]
        public DtrStatus Status { get; set; }
        
        [StringLength(500)]
        public string RejectionReason { get; set; }
    }
}