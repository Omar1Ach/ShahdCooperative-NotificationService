using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Queries.InAppNotification;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, IEnumerable<Domain.Entities.InAppNotification>>
{
    private readonly IInAppNotificationRepository _inAppRepository;

    public GetUserNotificationsQueryHandler(IInAppNotificationRepository inAppRepository)
    {
        _inAppRepository = inAppRepository ?? throw new ArgumentNullException(nameof(inAppRepository));
    }

    public async Task<IEnumerable<Domain.Entities.InAppNotification>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await _inAppRepository.GetUserNotificationsAsync(request.UserId, request.PageNumber, request.PageSize, cancellationToken);
    }
}
