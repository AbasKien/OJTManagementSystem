using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.Dtos
{
    public class RegisterInternDto
    {

            [Required(ErrorMessage = "First name is required")]
            [StringLength(100)]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            [StringLength(100)]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone(ErrorMessage = "Invalid phone number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required(ErrorMessage = "Confirm password is required")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Student ID is required")]
            [StringLength(50)]
            public string StudentId { get; set; }

            [Required(ErrorMessage = "School/University is required")]
            [StringLength(200)]
            public string School { get; set; }

            [Required(ErrorMessage = "Course is required")]
            [StringLength(200)]
            public string Course { get; set; }

            [Required(ErrorMessage = "Department is required")]
            [StringLength(500)]
            public string Department { get; set; }

            [Required(ErrorMessage = "Start date is required")]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; }

            [Required(ErrorMessage = "End date is required")]
            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; }

            [Required(ErrorMessage = "Supervisor is required")]
            public int SupervisorId { get; set; }
        }
    }


