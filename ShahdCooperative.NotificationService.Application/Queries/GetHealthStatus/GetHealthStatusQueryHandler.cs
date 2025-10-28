using MediatR;
using ShahdCooperative.NotificationService.Application.DTOs;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Queries.GetHealthStatus;

public class GetHealthStatusQueryHandler : IRequestHandler<GetHealthStatusQuery, HealthStatusDto>
{
    private readonly INotificationQueueRepository _queueRepository;

    public GetHealthStatusQueryHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<HealthStatusDto> Handle(GetHealthStatusQuery request, CancellationToken cancellationToken)
    {
        var isDatabaseHealthy = await CheckDatabaseHealthAsync(cancellationToken);
        var isQueueHealthy = await CheckQueueHealthAsync(cancellationToken);

        var overallStatus = isDatabaseHealthy && isQueueHealthy ? "Healthy" : "Unhealthy";

        return new HealthStatusDto
        {
            Status = overallStatus,
            IsDatabaseHealthy = isDatabaseHealthy,
            IsQueueHealthy = isQueueHealthy,
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        };
    }

    private async Task<bool> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _queueRepository.GetPendingNotificationsAsync(1, 3, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckQueueHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _queueRepository.GetPendingNotificationsAsync(1, 3, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
