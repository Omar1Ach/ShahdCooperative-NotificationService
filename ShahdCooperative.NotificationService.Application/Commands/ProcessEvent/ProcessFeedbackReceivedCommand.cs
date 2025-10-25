using MediatR;
using ShahdCooperative.NotificationService.Domain.Events;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessFeedbackReceivedCommand : IRequest<bool>
{
    public FeedbackReceivedEvent Event { get; set; } = null!;
}
