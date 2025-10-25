using MediatR;
using ShahdCooperative.NotificationService.Domain.Events;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessOrderCreatedCommand : IRequest<bool>
{
    public OrderCreatedEvent Event { get; set; } = null!;
}
