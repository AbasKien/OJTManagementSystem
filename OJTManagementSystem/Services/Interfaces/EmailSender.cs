using System.Net;
using System.Net.Mail;
using OJTManagementSystem.Services.Interfaces;


namespace OJTManagementSystem.Services.Interfaces
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var smtp = new SmtpClient
            {
                Host = _config["Email:Smtp"],
                Port = int.Parse(_config["Email:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["Email:Username"],
                    _config["Email:Password"])
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:From"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);
            await smtp.SendMailAsync(mail);
        }
    }
}
