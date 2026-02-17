using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OJTManagementSystem.Data;
using OJTManagementSystem.Helpers;
using OJTManagementSystem.Models;
using OJTManagementSystem.Services.Interfaces;
using OJTManagementSystem.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

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
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                // ✅ Block login if email is not confirmed
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    TempData["InfoMessage"] =
                        "You must confirm your email before logging in. " +
                        "Please check your inbox (and spam folder).";
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact support.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);

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
            catch (Exception)
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
                model.Role = "Intern";

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

                // ✅ Create user — EmailConfirmed stays false by default (requires verification)
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

                // ✅ Send email confirmation link
                await SendConfirmationEmailAsync(user);

                // ✅ Show "check your email" screen instead of redirecting to login
                TempData["RegistrationSuccess"] = true;
                TempData["RegisteredEmail"] = user.Email;
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // ============================================================
        // CONFIRM EMAIL
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["EmailConfirmed"] = false;
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["EmailConfirmed"] = false;
                return View();
            }

            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            TempData["EmailConfirmed"] = result.Succeeded;
            return View();
        }

        // ============================================================
        // RESEND CONFIRMATION EMAIL
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    await SendConfirmationEmailAsync(user);
                }
            }

            // Always show the same screen (don't reveal whether email exists)
            TempData["RegistrationSuccess"] = true;
            TempData["RegisteredEmail"] = email;
            TempData["SuccessMessage"] = "Confirmation email resent. Please check your inbox.";
            return RedirectToAction("RegisterIntern");
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
            catch (Exception)
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
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // ✅ CRITICAL FIX: Clear session on logout
            HttpContext.Session.Clear();

            TempData["Success"] = "You have been successfully logged out.";
            return RedirectToAction("Login");
        }

        // Line 378 - GET Logout
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogoutGet()
        {
            await _signInManager.SignOutAsync();

            // ✅ CRITICAL FIX: Clear session on logout
            HttpContext.Session.Clear();

            TempData["Success"] = "You have been successfully logged out.";
            return RedirectToAction("Login");
        }

        // ============================================================
        // FORGOT PASSWORD
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
                    ModelState.AddModelError("email", "Email is required.");
                    return View();
                }

                var user = await _userManager.FindByEmailAsync(email);

                // ✅ Only send if user exists AND email is confirmed (no reset for unverified accounts)
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var encoded = HttpUtility.UrlEncode(token);
                    var resetLink = Url.Action(
                        action: "ResetPassword",
                        controller: "Account",
                        values: new { token = encoded, email = user.Email },
                        protocol: Request.Scheme);

                    var emailBody = $@"
                        <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                            <div style='background:linear-gradient(135deg,#6f42c1,#563d7c);padding:30px;border-radius:10px 10px 0 0;text-align:center;'>
                                <h2 style='color:#fff;margin:0;'>🔐 Password Reset</h2>
                                <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;'>OJT Management System</p>
                            </div>
                            <div style='background:#fff;padding:32px;border:1px solid #e5e7eb;border-radius:0 0 10px 10px;'>
                                <p style='font-size:15px;color:#374151;'>Hello <strong>{user.FullName}</strong>,</p>
                                <p style='color:#6b7280;'>We received a request to reset your password. Click the button below to create a new one.</p>
                                <div style='text-align:center;margin:30px 0;'>
                                    <a href='{resetLink}'
                                       style='background:linear-gradient(135deg,#6f42c1,#563d7c);color:#fff;
                                              padding:14px 32px;border-radius:8px;text-decoration:none;
                                              font-weight:bold;font-size:15px;display:inline-block;'>
                                        🔑 Reset My Password
                                    </a>
                                </div>
                                <p style='font-size:13px;color:#9ca3af;text-align:center;'>
                                    This link expires in <strong>1 hour</strong>. If you did not request this, you can safely ignore this email.
                                </p>
                            </div>
                        </div>";

                    await _emailService.SendEmailAsync(user.Email, "Reset Your OJT Account Password", emailBody);
                }

                // ✅ Always show success — never reveal whether the email exists (security best practice)
                TempData["SuccessMessage"] =
                    $"If an account exists for {email}, a password reset link has been sent. Please check your inbox.";
                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
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
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return View();
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
                    // Don't reveal user doesn't exist — just show success
                    TempData["SuccessMessage"] = "Your password has been reset. You can now log in.";
                    return View(model);
                }

                var decodedToken = HttpUtility.UrlDecode(model.Token);
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password has been reset successfully. You can now login with your new password.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Password reset failed. The link may have expired. Please request a new one.";
                }

                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
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

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        /// <summary>
        /// Generates an email confirmation token and sends the confirmation email.
        /// </summary>
        private async Task SendConfirmationEmailAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encoded = HttpUtility.UrlEncode(token);
            var callbackUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "Account",
                values: new { userId = user.Id, token = encoded },
                protocol: Request.Scheme);

            var emailBody = $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                    <div style='background:linear-gradient(135deg,#198754,#155724);padding:30px;border-radius:10px 10px 0 0;text-align:center;'>
                        <h2 style='color:#fff;margin:0;'>✉️ Confirm Your Email</h2>
                        <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;'>OJT Management System</p>
                    </div>
                    <div style='background:#fff;padding:32px;border:1px solid #e5e7eb;border-radius:0 0 10px 10px;'>
                        <p style='font-size:15px;color:#374151;'>Hello <strong>{user.FullName}</strong>,</p>
                        <p style='color:#6b7280;'>Thank you for registering. Please confirm your email address to activate your account.</p>
                        <div style='text-align:center;margin:30px 0;'>
                            <a href='{callbackUrl}'
                               style='background:linear-gradient(135deg,#198754,#155724);color:#fff;
                                      padding:14px 32px;border-radius:8px;text-decoration:none;
                                      font-weight:bold;font-size:15px;display:inline-block;'>
                                ✅ Confirm Email Address
                            </a>
                        </div>
                        <p style='font-size:13px;color:#9ca3af;text-align:center;'>
                            This link expires in <strong>24 hours</strong>. If you did not register, you can safely ignore this email.
                        </p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your OJT Account Email",
                emailBody);
        }
    }

    // ============================================================
    // RESET PASSWORD VIEW MODEL
    // ============================================================

    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}