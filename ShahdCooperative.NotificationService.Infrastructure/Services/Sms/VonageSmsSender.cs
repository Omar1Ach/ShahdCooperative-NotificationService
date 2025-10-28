using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using Vonage;
using Vonage.Messaging;
using Vonage.Request;

namespace ShahdCooperative.NotificationService.Infrastructure.Services.Sms;

public class VonageSmsSender : INotificationSender
{
    private readonly ILogger<VonageSmsSender> _logger;
    private readonly SmsSettings _settings;
    private readonly VonageClient _vonageClient;

    public VonageSmsSender(
        ILogger<VonageSmsSender> logger,
        IOptions<SmsSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        if (string.IsNullOrWhiteSpace(_settings.VonageApiKey) ||
            string.IsNullOrWhiteSpace(_settings.VonageApiSecret))
        {
            throw new InvalidOperationException("Vonage credentials are not configured");
        }

        var credentials = Credentials.FromApiKeyAndSecret(
            _settings.VonageApiKey,
            _settings.VonageApiSecret);

        _vonageClient = new VonageClient(credentials);
    }

    public NotificationType NotificationType => NotificationType.SMS;

    public async Task<bool> SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                _logger.LogWarning("Recipient phone number is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("SMS body is empty");
                return false;
            }

            var request = new SendSmsRequest
            {
                To = recipient,
                From = _settings.VonageFromNumber,
                Text = body
            };

            var response = await _vonageClient.SmsClient.SendAnSmsAsync(request);

            if (response.Messages[0].Status != "0")
            {
                _logger.LogError("Vonage SMS failed with status {Status}: {ErrorText}",
                    response.Messages[0].Status, response.Messages[0].ErrorText);
                return false;
            }

            _logger.LogInformation("SMS sent successfully via Vonage to {Recipient}, MessageId: {MessageId}",
                recipient, response.Messages[0].MessageId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS via Vonage to {Recipient}", recipient);
            return false;
        }
    }
}
