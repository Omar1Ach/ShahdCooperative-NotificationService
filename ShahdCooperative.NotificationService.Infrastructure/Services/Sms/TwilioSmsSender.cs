using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ShahdCooperative.NotificationService.Infrastructure.Services.Sms;

public class TwilioSmsSender : INotificationSender
{
    private readonly ILogger<TwilioSmsSender> _logger;
    private readonly SmsSettings _settings;
    private readonly bool _isConfigured;

    public TwilioSmsSender(
        ILogger<TwilioSmsSender> logger,
        IOptions<SmsSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        if (string.IsNullOrWhiteSpace(_settings.TwilioAccountSid) ||
            string.IsNullOrWhiteSpace(_settings.TwilioAuthToken))
        {
            _logger.LogWarning("Twilio credentials are not configured. SMS sending will be disabled.");
            _isConfigured = false;
        }
        else
        {
            TwilioClient.Init(_settings.TwilioAccountSid, _settings.TwilioAuthToken);
            _isConfigured = true;
        }
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
            if (!_isConfigured)
            {
                _logger.LogWarning("Twilio SMS sender is not configured. Cannot send SMS.");
                return false;
            }

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

            var message = await MessageResource.CreateAsync(
                body: body,
                from: new PhoneNumber(_settings.TwilioPhoneNumber),
                to: new PhoneNumber(recipient)
            );

            if (message.ErrorCode.HasValue)
            {
                _logger.LogError("Twilio SMS failed with error code {ErrorCode}: {ErrorMessage}",
                    message.ErrorCode, message.ErrorMessage);
                return false;
            }

            _logger.LogInformation("SMS sent successfully via Twilio to {Recipient}, SID: {MessageSid}",
                recipient, message.Sid);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS via Twilio to {Recipient}", recipient);
            return false;
        }
    }
}
