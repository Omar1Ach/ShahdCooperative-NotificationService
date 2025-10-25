using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface IDeviceTokenRepository
{
    Task<Guid> RegisterTokenAsync(DeviceToken deviceToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceToken>> GetActiveTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceToken>> GetUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeactivateTokenAsync(Guid tokenId, CancellationToken cancellationToken = default);
    Task DeleteTokenAsync(Guid tokenId, CancellationToken cancellationToken = default);
    Task UpdateLastUsedAsync(Guid tokenId, CancellationToken cancellationToken = default);
}
