using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface INotificationQueueRepository
{
    Task<Guid> EnqueueAsync(NotificationQueue notification, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationQueue>> GetPendingNotificationsAsync(int batchSize, int maxRetries, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, NotificationStatus status, CancellationToken cancellationToken = default);
    Task IncrementAttemptAsync(Guid id, string? errorMessage, CancellationToken cancellationToken = default);
    Task SetNextRetryAsync(Guid id, DateTime nextRetryAt, CancellationToken cancellationToken = default);
    Task<NotificationQueue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationQueue>> GetScheduledNotificationsAsync(DateTime currentTime, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationQueue>> GetFailedNotificationsForRetryAsync(DateTime currentTime, CancellationToken cancellationToken = default);
}
