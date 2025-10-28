using MediatR;

namespace ShahdCooperative.NotificationService.Application.Commands.InAppNotification;

public class MarkAsReadCommand : IRequest<bool>
{
    public Guid NotificationId { get; set; }
}
