using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface INotificationLogRepository
{
    Task<Guid> CreateAsync(NotificationLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationLog>> GetLogsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationLog>> GetLogsByRecipientAsync(string recipient, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
