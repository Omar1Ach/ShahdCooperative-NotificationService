using MediatR;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Commands.InAppNotification;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, bool>
{
    private readonly IInAppNotificationRepository _inAppRepository;

    public MarkAllAsReadCommandHandler(IInAppNotificationRepository inAppRepository)
    {
        _inAppRepository = inAppRepository ?? throw new ArgumentNullException(nameof(inAppRepository));
    }

    public async Task<bool> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        await _inAppRepository.MarkAllAsReadAsync(request.UserId, cancellationToken);
        return true;
    }
}
