using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Helpers.Exceptions;
using Service.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings["FromName"] ?? "Janz", settings["From"] ?? user));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = html };

            try
            {
                // MailKit: 465 portunda dərhal SSL, qalanında (587) STARTTLS
                var port = int.TryParse(settings["Port"], out var parsed) ? parsed : 587;
                using var client = new SmtpClient();
                await client.ConnectAsync(host, port, port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(user, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Səbəb loga yazılır; lokal işləyəndə cavabda da göstərilir ki, düzəltmək asan olsun (parol heç vaxt yazılmır)
                _logger.LogError(ex, "Could not send the email to {To} through {Host}", to, host);
                var local = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
                if (local)
                {
                    // Lokal işləyəndə SMTP səhvi qeydiyyatı dayandırmasın: məktubun məzmunu (kod/link) loga yazılır
                    _logger.LogWarning("Email was not sent ({Reason}). Message to {To}: {Subject}\n{Html}", Describe(ex), to, subject, html);
                    return;
                }

                throw new BadRequestException("We could not send the email right now. Please try again later.");
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
