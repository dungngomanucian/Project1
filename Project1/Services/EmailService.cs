using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Project1.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;  // Thêm logger

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(
                    _configuration["EmailSettings:DisplayName"],
                    _configuration["EmailSettings:Email"]
                ));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = body;
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                _logger.LogInformation($"Connecting to {_configuration["EmailSettings:Host"]}:{_configuration["EmailSettings:Port"]}");

                await smtp.ConnectAsync(
                    _configuration["EmailSettings:Host"],
                    int.Parse(_configuration["EmailSettings:Port"]),
                    SecureSocketOptions.StartTls
                );

                _logger.LogInformation("Authenticating...");

                await smtp.AuthenticateAsync(
                    _configuration["EmailSettings:Email"],
                    _configuration["EmailSettings:Password"]
                );

                _logger.LogInformation("Sending email...");
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw new Exception($"Failed to send email: {ex.Message}");
            }
        }
    }
}
