using MediatR;
using ShahdCooperative.NotificationService.Domain.Events;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessProductOutOfStockCommand : IRequest<bool>
{
    public ProductOutOfStockEvent Event { get; set; } = null!;
}
