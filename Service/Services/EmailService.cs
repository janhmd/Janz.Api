using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Helpers.Exceptions;
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

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex) when (ex is SmtpException or InvalidOperationException or System.IO.IOException)
            {
                // Səbəb loga yazılır; lokal işləyəndə cavabda da göstərilir ki, düzəltmək asan olsun (parol heç vaxt yazılmır)
                _logger.LogError(ex, "Could not send the email to {To} through {Host}", to, host);
                var local = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
                throw new BadRequestException(local
                    ? $"The email could not be sent ({Describe(ex)})"
                    : "We could not send the email right now. Please try again later.");
            }
        }

        // Xarici SmtpException çox vaxt yalnız "Failure sending mail." deyir, əsl səbəb içdəki xətalardadır
        private static string Describe(Exception ex)
        {
            var parts = new List<string>();
            for (var current = ex; current is not null; current = current.InnerException)
            {
                parts.Add($"{current.GetType().Name}: {current.Message}");
            }

            return string.Join(" -> ", parts);
        }
    }
}
