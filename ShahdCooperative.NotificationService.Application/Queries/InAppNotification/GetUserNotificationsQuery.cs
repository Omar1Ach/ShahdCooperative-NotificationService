using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Application.Queries.InAppNotification;

public class GetUserNotificationsQuery : IRequest<IEnumerable<Domain.Entities.InAppNotification>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
