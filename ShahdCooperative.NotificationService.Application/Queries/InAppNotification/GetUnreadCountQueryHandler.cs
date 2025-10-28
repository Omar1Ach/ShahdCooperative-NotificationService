using MediatR;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Queries.InAppNotification;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly IInAppNotificationRepository _inAppRepository;

    public GetUnreadCountQueryHandler(IInAppNotificationRepository inAppRepository)
    {
        _inAppRepository = inAppRepository ?? throw new ArgumentNullException(nameof(inAppRepository));
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await _inAppRepository.GetUnreadCountAsync(request.UserId, cancellationToken);
    }
}
