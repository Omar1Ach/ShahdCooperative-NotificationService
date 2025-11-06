using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface IInAppNotificationRepository
{
    Task<Guid> CreateAsync(InAppNotification notification, CancellationToken cancellationToken = default);
    Task<IEnumerable<InAppNotification>> GetUserNotificationsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredNotificationsAsync(DateTime currentTime, CancellationToken cancellationToken = default);
}
