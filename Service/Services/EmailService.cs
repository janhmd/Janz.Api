using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Service.Services
{
    // Məktublar SMTP ilə gedir (appsettings -> "Email"; Gmail üçün App Password user-secrets-də saxlanılır).
    // SMTP yazılmayıbsa məktub göndərilmir, məzmunu loga yazılır: lokal işləyəndə link oradan götürülə bilir
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string html)
        {
            var settings = _configuration.GetSection("Email");
            var host = settings["Host"];
            var user = settings["User"];
            var password = settings["Password"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Email is not configured. Message to {To}: {Subject}\n{Html}", to, subject, html);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(settings["From"] ?? user, settings["FromName"] ?? "Janz"),
                Subject = subject,
                Body = html,
                IsBodyHtml = true
            };
            message.To.Add(to);

            using var client = new SmtpClient(host, int.TryParse(settings["Port"], out var port) ? port : 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, password)
            };

            await client.SendMailAsync(message);
        }
    }
}
