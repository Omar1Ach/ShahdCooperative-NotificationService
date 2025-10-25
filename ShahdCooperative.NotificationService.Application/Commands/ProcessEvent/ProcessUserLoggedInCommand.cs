using MediatR;
using ShahdCooperative.NotificationService.Domain.Events;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessUserLoggedInCommand : IRequest<bool>
{
    public UserLoggedInEvent Event { get; set; } = null!;
}
