using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using System.Net;
using System.Net.Mail;

namespace ShahdCooperative.NotificationService.Infrastructure.Services.Email;

public class SmtpEmailSender : INotificationSender
{
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly EmailSettings _settings;

    public NotificationType NotificationType => NotificationType.Email;

    public SmtpEmailSender(ILogger<SmtpEmailSender> logger, IOptions<EmailSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<bool> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
            {
                _logger.LogError("SMTP host is not configured");
                return false;
            }

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(recipient);

            await client.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("SMTP email sent successfully to {Recipient}", recipient);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMTP email to {Recipient}", recipient);
            return false;
        }
    }
}
