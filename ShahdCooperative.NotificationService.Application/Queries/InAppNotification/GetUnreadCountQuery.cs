using MediatR;

namespace ShahdCooperative.NotificationService.Application.Queries.InAppNotification;

public class GetUnreadCountQuery : IRequest<int>
{
    public Guid UserId { get; set; }
}
