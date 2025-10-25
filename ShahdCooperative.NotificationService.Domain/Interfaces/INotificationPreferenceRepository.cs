using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(NotificationPreference preference, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
