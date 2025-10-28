using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;

namespace ShahdCooperative.NotificationService.Infrastructure.Services.Push;

public class FirebasePushNotificationSender : INotificationSender
{
    private readonly ILogger<FirebasePushNotificationSender> _logger;
    private readonly PushNotificationSettings _settings;
    private readonly FirebaseApp _firebaseApp;

    public FirebasePushNotificationSender(
        ILogger<FirebasePushNotificationSender> logger,
        IOptions<PushNotificationSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        if (string.IsNullOrWhiteSpace(_settings.FirebaseCredentialsPath))
        {
            throw new InvalidOperationException("Firebase credentials path is not configured");
        }

        if (!File.Exists(_settings.FirebaseCredentialsPath))
        {
            throw new InvalidOperationException($"Firebase credentials file not found at: {_settings.FirebaseCredentialsPath}");
        }

        try
        {
            // Check if app already exists
            _firebaseApp = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(_settings.FirebaseCredentialsPath),
                ProjectId = _settings.FirebaseProjectId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase app");
            throw new InvalidOperationException("Failed to initialize Firebase app", ex);
        }
    }

    public NotificationType NotificationType => NotificationType.Push;

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
                _logger.LogWarning("Recipient device token is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("Push notification body is empty");
                return false;
            }

            var message = new Message
            {
                Token = recipient,
                Notification = new Notification
                {
                    Title = subject ?? "Notification",
                    Body = body
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Title = subject ?? "Notification",
                        Body = body,
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = subject ?? "Notification",
                            Body = body
                        },
                        Badge = 1,
                        Sound = "default"
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);

            _logger.LogInformation("Push notification sent successfully via Firebase FCM to device token ending with {TokenSuffix}, MessageId: {MessageId}",
                recipient.Length > 10 ? recipient.Substring(recipient.Length - 10) : recipient, response);

            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase FCM error sending push notification. ErrorCode: {ErrorCode}", ex.MessagingErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification via Firebase FCM");
            return false;
        }
    }
}
