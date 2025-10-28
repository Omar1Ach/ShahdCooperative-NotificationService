using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;

namespace ShahdCooperative.NotificationService.Infrastructure.Services.Email;

public class AwsSesEmailSender : INotificationSender
{
    private readonly ILogger<AwsSesEmailSender> _logger;
    private readonly EmailSettings _settings;

    public Domain.Enums.NotificationType NotificationType => Domain.Enums.NotificationType.Email;

    public AwsSesEmailSender(ILogger<AwsSesEmailSender> logger, IOptions<EmailSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<bool> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.AwsSesAccessKey) ||
                string.IsNullOrWhiteSpace(_settings.AwsSesSecretKey))
            {
                _logger.LogError("AWS SES credentials are not configured");
                return false;
            }

            var region = RegionEndpoint.GetBySystemName(_settings.AwsSesRegion);
            using var client = new AmazonSimpleEmailServiceClient(
                _settings.AwsSesAccessKey,
                _settings.AwsSesSecretKey,
                region);

            var sendRequest = new SendEmailRequest
            {
                Source = $"{_settings.FromName} <{_settings.FromEmail}>",
                Destination = new Destination
                {
                    ToAddresses = new List<string> { recipient }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = body
                        }
                    }
                }
            };

            var response = await client.SendEmailAsync(sendRequest, cancellationToken);

            _logger.LogInformation("AWS SES email sent successfully to {Recipient}. MessageId: {MessageId}",
                recipient, response.MessageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send AWS SES email to {Recipient}", recipient);
            return false;
        }
    }
}
