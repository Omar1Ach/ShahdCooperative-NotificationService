using MediatR;
using ShahdCooperative.NotificationService.Domain.Events;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessOrderShippedCommand : IRequest<bool>
{
    public OrderShippedEvent Event { get; set; } = null!;
}
