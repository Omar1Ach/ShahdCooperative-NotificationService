using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;

namespace ShahdCooperative.NotificationService.Infrastructure.Services.Email;

public class SendGridEmailSender : INotificationSender
{
    private readonly ILogger<SendGridEmailSender> _logger;
    private readonly EmailSettings _settings;

    public NotificationType NotificationType => NotificationType.Email;

    public SendGridEmailSender(ILogger<SendGridEmailSender> logger, IOptions<EmailSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<bool> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.SendGridApiKey))
            {
                _logger.LogError("SendGrid API key is not configured");
                return false;
            }

            var client = new SendGridClient(_settings.SendGridApiKey);

            var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
            var to = new EmailAddress(recipient);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

            var response = await client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SendGrid email sent successfully to {Recipient}", recipient);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("SendGrid email failed with status {StatusCode}: {Response}",
                    response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SendGrid email to {Recipient}", recipient);
            return false;
        }
    }
}
