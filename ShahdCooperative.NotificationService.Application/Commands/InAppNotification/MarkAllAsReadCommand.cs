using MediatR;

namespace ShahdCooperative.NotificationService.Application.Commands.InAppNotification;

public class MarkAllAsReadCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
}
