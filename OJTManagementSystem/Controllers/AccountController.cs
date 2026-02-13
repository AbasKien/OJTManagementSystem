using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OJTManagementSystem.Data;
using OJTManagementSystem.Helpers;
using OJTManagementSystem.Models;
using OJTManagementSystem.Services.Interfaces;
using OJTManagementSystem.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace OJTManagementSystem.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _context = context;
        }

        // ============================================================
        // LOGIN
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Supervisor"))
                    return RedirectToAction("Dashboard", "Supervisor");

                if (roles.Contains("Intern"))
                    return RedirectToAction("Dashboard", "Intern");
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (!user.IsActive)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact support.");
                        return View(model);
                    }

                    if (roles.Contains("Supervisor"))
                        return RedirectToAction("Dashboard", "Supervisor");
                    else if (roles.Contains("Intern"))
                        return RedirectToAction("Dashboard", "Intern");
                    else
                        return RedirectToAction("Login", "Account");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been locked. Please try again later.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        // ============================================================
        // REGISTER - INTERN
        // ============================================================

        [HttpGet]
        public IActionResult RegisterIntern()
        {
            var model = new RegisterViewModel
            {
                Role = "Intern",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(6)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterIntern(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var userExists = await _userManager.FindByEmailAsync(model.Email);
                if (userExists != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(model);
                }

                var duration = (model.EndDate - model.StartDate).TotalDays;
                if (duration < 0)
                {
                    ModelState.AddModelError("EndDate", "Internship must be at least 30 days.");
                    return View(model);
                }

                if (duration > 365)
                {
                    ModelState.AddModelError("EndDate", "Internship cannot exceed 1 year.");
                    return View(model);
                }

                var user = MappingHelper.MapRegisterViewModelToApplicationUser(model);

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, "Intern");

                var internDto = MappingHelper.MapRegisterViewModelToDto(model);
                var intern = MappingHelper.MapRegisterInternDtoToIntern(internDto, user.Id);

                _context.Interns.Add(intern);
                await _context.SaveChangesAsync();

                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName, "Intern");
                }
                catch
                {
                    // Email failed but registration succeeded
                }

                TempData["Success"] = "Registration successful! You can now login. A supervisor will be assigned to you shortly.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // ============================================================
        // REGISTER - SUPERVISOR
        // ============================================================

        [HttpGet]
        public IActionResult RegisterSupervisor()
        {
            var model = new RegisterViewModel
            {
                Role = "Supervisor"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSupervisor(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var userExists = await _userManager.FindByEmailAsync(model.Email);
                if (userExists != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Position))
                {
                    ModelState.AddModelError("Position", "Position is required for supervisors.");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Department))
                {
                    ModelState.AddModelError("Department", "Department is required for supervisors.");
                    return View(model);
                }

                var user = MappingHelper.MapRegisterViewModelToApplicationUser(model);

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, "Supervisor");

                var supervisor = MappingHelper.MapRegisterViewModelToSupervisor(model, user.Id);

                _context.Supervisors.Add(supervisor);
                await _context.SaveChangesAsync();

                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName, "Supervisor");
                }
                catch
                {
                    // Email failed but registration succeeded
                }

                TempData["Success"] = "Registration successful! You can now login and start managing interns.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // ============================================================
        // REGISTER - GENERIC
        // ============================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string role)
        {
            if (role == "Intern")
                return RedirectToAction("RegisterIntern");
            else if (role == "Supervisor")
                return RedirectToAction("RegisterSupervisor");
            else
                return RedirectToAction("Register");
        }

        // ============================================================
        // LOGOUT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "You have been successfully logged out.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogoutGet()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "You have been successfully logged out.";
            return RedirectToAction("Login");
        }

        // ============================================================
        // FORGOT PASSWORD (Optional - depends on your IEmailService)
        // ============================================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    ModelState.AddModelError("Email", "Email is required.");
                    return View();
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal that the user doesn't exist
                    TempData["Success"] = "If an account with that email exists, a password reset link has been sent.";
                    return RedirectToAction("Login");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

                // Only send email if your IEmailService has this method
                // Otherwise, remove this try-catch block
                try
                {
                    // If SendPasswordResetEmailAsync doesn't exist, comment this out
                    // await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);

                    // Alternative: use a generic email method if available
                    // await _emailService.SendEmailAsync(user.Email, "Password Reset", $"Reset your password: {resetLink}");
                }
                catch
                {
                    TempData["Error"] = "Failed to send password reset email. Please try again later.";
                    return View();
                }

                TempData["Success"] = "If an account with that email exists, a password reset link has been sent.";
                return RedirectToAction("Login");
            }
            catch
            {
                TempData["Error"] = "An error occurred. Please try again.";
                return View();
            }
        }

        // ============================================================
        // RESET PASSWORD
        // ============================================================

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    TempData["Error"] = "Invalid password reset attempt.";
                    return RedirectToAction("Login");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }

                TempData["Success"] = "Password has been reset successfully. You can now login with your new password.";
                return RedirectToAction("Login");
            }
            catch
            {
                TempData["Error"] = "An error occurred. Please try again.";
                return View(model);
            }
        }

        // ============================================================
        // ACCESS DENIED
        // ============================================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    // ============================================================
    // RESET PASSWORD VIEW MODEL
    // ============================================================

    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}