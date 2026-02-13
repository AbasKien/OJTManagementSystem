using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.ViewModel
{
    public class RegisterViewModel
    {
        // ======================
        // COMMON FIELDS
        // ======================

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        // ======================
        // INTERN-SPECIFIC FIELDS
        // ======================

        [Required(ErrorMessage = "Student ID is required")]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string School { get; set; } = string.Empty;

        [Required]
        public string Course { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // ======================
        // SUPERVISOR-SPECIFIC FIELDS
        // ======================

        public string? Position { get; set; }
    }
}
