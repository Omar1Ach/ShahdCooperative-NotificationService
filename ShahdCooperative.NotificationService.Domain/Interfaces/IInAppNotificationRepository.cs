using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface IInAppNotificationRepository
{
    Task<Guid> CreateAsync(InAppNotification notification, CancellationToken cancellationToken = default);
    Task<IEnumerable<InAppNotification>> GetUserNotificationsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
