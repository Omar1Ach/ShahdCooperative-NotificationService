using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplate>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
