using Microsoft.AspNetCore.Identity;

namespace OJTManagementSystem.Models
{
    public class Account
    {
        public class ApplicationUser : IdentityUser
        {
            public string? FullName { get; set; }
        }
    }
}
