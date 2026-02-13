using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using OJTManagementSystem.Helpers;
using OJTManagementSystem.Services.Interfaces;

namespace OJTManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            return await SendEmailAsync(to, subject, body, _emailSettings.SenderName);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, string displayName)
        {
            try
            {
                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                    client.EnableSsl = true;

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_emailSettings.SenderEmail, displayName);
                        message.To.Add(to);
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = true;

                        await client.SendMailAsync(message);
                    }
                }

                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {to}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string fullName, string role)
        {
            try
            {
                var subject = "Welcome to OJT Management System";
                var body = $@"
                    <h2>Welcome, {fullName}!</h2>
                    <p>Your account has been successfully created in the OJT Management System.</p>
                    <p><strong>Your Role:</strong> {role}</p>
                    <p>You can now login at: <a href='https://ojtsystem.com/Account/Login'>Login</a></p>
                    <p>If you have any questions, please contact support.</p>
                    <p>Best regards,<br>OJT Management System Team</p>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending welcome email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendDtrApprovalEmailAsync(string email, string internName, string status)
        {
            try
            {
                var subject = $"DTR {status} - OJT Management System";
                var body = $@"
                    <h2>Daily Time Record {status}</h2>
                    <p>Hello,</p>
                    <p>Your DTR submission has been <strong>{status}</strong> by your supervisor.</p>
                    <p><strong>Intern Name:</strong> {internName}</p>
                    <p>Please login to the system to view details.</p>
                    <p>Best regards,<br>OJT Management System Team</p>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending DTR approval email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendLeaveApprovalEmailAsync(string email, string internName, string status)
        {
            try
            {
                var subject = $"Leave Request {status} - OJT Management System";
                var body = $@"
                    <h2>Leave Request {status}</h2>
                    <p>Hello,</p>
                    <p>Your leave request has been <strong>{status}</strong> by your supervisor.</p>
                    <p><strong>Intern Name:</strong> {internName}</p>
                    <p>Please login to the system to view details.</p>
                    <p>Best regards,<br>OJT Management System Team</p>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending leave approval email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEvaluationNotificationAsync(string email, string internName, decimal rating)
        {
            try
            {
                var subject = "Performance Evaluation Completed - OJT Management System";
                var body = $@"
                    <h2>Performance Evaluation Completed</h2>
                    <p>Hello {internName},</p>
                    <p>Your performance evaluation has been completed by your supervisor.</p>
                    <p><strong>Final Rating:</strong> {rating:F2} / 5.0</p>
                    <p>Please login to the system to view your complete evaluation.</p>
                    <p>Best regards,<br>OJT Management System Team</p>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending evaluation notification email: {ex.Message}");
                return false;
            }
        }
    }
}