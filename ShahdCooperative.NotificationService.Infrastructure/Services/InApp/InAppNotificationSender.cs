using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Services.InApp;

public class InAppNotificationSender : INotificationSender
{
    private readonly ILogger<InAppNotificationSender> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationHubClient _hubClient;

    public InAppNotificationSender(
        ILogger<InAppNotificationSender> logger,
        IServiceProvider serviceProvider,
        INotificationHubClient hubClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _hubClient = hubClient ?? throw new ArgumentNullException(nameof(hubClient));
    }

    public NotificationType NotificationType => NotificationType.InApp;

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
                _logger.LogWarning("Recipient user ID is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("In-app notification body is empty");
                return false;
            }

            // Parse user ID
            if (!Guid.TryParse(recipient, out var userId))
            {
                _logger.LogWarning("Invalid user ID format: {Recipient}", recipient);
                return false;
            }

            // Create notification in database
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IInAppNotificationRepository>();

            var notification = new InAppNotification
            {
                UserId = userId,
                Title = subject ?? "Notification",
                Message = body,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var notificationId = await repository.CreateAsync(notification, cancellationToken);

            // Send real-time notification via SignalR (if hub client is available)
            await _hubClient.SendNotificationToUserAsync(recipient, new
            {
                id = notificationId,
                title = notification.Title,
                message = notification.Message,
                createdAt = notification.CreatedAt,
                isRead = false
            });

            _logger.LogInformation("In-app notification sent successfully to user {UserId}, NotificationId: {NotificationId}",
                recipient, notificationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send in-app notification to user {UserId}", recipient);
            return false;
        }
    }
}
