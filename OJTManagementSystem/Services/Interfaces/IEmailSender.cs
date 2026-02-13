namespace OJTManagementSystem.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendEmailAsync(string to, string subject, string body, string displayName);
        Task<bool> SendWelcomeEmailAsync(string email, string fullName, string role);
        Task<bool> SendDtrApprovalEmailAsync(string email, string internName, string status);
        Task<bool> SendLeaveApprovalEmailAsync(string email, string internName, string status);
        Task<bool> SendEvaluationNotificationAsync(string email, string internName, decimal rating);
    }
}