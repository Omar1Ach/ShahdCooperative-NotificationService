using MediatR;
using ShahdCooperative.NotificationService.Domain.Events;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessUserRegisteredCommand : IRequest<bool>
{
    public UserRegisteredEvent Event { get; set; } = null!;
}
