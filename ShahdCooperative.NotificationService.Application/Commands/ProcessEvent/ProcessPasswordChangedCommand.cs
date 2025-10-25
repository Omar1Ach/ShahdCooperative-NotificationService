using MediatR;
using ShahdCooperative.NotificationService.Domain.Events;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessPasswordChangedCommand : IRequest<bool>
{
    public PasswordChangedEvent Event { get; set; } = null!;
}
