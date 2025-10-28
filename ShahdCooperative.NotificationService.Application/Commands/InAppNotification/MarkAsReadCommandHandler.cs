using MediatR;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Commands.InAppNotification;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, bool>
{
    private readonly IInAppNotificationRepository _inAppRepository;

    public MarkAsReadCommandHandler(IInAppNotificationRepository inAppRepository)
    {
        _inAppRepository = inAppRepository ?? throw new ArgumentNullException(nameof(inAppRepository));
    }

    public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        await _inAppRepository.MarkAsReadAsync(request.NotificationId, cancellationToken);
        return true;
    }
}
