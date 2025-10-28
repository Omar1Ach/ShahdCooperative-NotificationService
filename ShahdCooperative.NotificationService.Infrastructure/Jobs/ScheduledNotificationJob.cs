using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Jobs;

public class ScheduledNotificationJob
{
    private readonly ILogger<ScheduledNotificationJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ScheduledNotificationJob(
        ILogger<ScheduledNotificationJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessScheduledNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Processing scheduled notifications...");

            using var scope = _serviceProvider.CreateScope();
            var queueRepository = scope.ServiceProvider.GetRequiredService<INotificationQueueRepository>();

            // Get notifications that are scheduled to be sent now
            var scheduledNotifications = await queueRepository.GetScheduledNotificationsAsync(DateTime.UtcNow);

            var notifications = scheduledNotifications.ToList();
            if (notifications.Count == 0)
            {
                _logger.LogInformation("No scheduled notifications to process");
                return;
            }

            _logger.LogInformation("Found {Count} scheduled notifications to process", notifications.Count);

            // Update status to Pending so queue processor can pick them up
            foreach (var notification in notifications)
            {
                await queueRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Pending);
            }

            _logger.LogInformation("Scheduled notifications moved to pending queue");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled notifications");
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Cleaning up expired notifications...");

            using var scope = _serviceProvider.CreateScope();
            var inAppRepository = scope.ServiceProvider.GetRequiredService<IInAppNotificationRepository>();

            var expiredCount = await inAppRepository.DeleteExpiredNotificationsAsync(DateTime.UtcNow);

            _logger.LogInformation("Deleted {Count} expired in-app notifications", expiredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired notifications");
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task RetryFailedNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Retrying failed notifications...");

            using var scope = _serviceProvider.CreateScope();
            var queueRepository = scope.ServiceProvider.GetRequiredService<INotificationQueueRepository>();

            // Get failed notifications that are eligible for retry (NextRetryAt has passed)
            var failedNotifications = await queueRepository.GetFailedNotificationsForRetryAsync(DateTime.UtcNow);

            var notifications = failedNotifications.ToList();
            if (notifications.Count == 0)
            {
                _logger.LogInformation("No failed notifications to retry");
                return;
            }

            _logger.LogInformation("Found {Count} failed notifications to retry", notifications.Count);

            // Update status to Pending for retry
            foreach (var notification in notifications)
            {
                await queueRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Pending);
            }

            _logger.LogInformation("Failed notifications moved to pending queue for retry");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed notifications");
            throw;
        }
    }
}
