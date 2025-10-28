using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Commands.SendNotification;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Guid>
{
    private readonly INotificationQueueRepository _queueRepository;

    public SendNotificationCommandHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<Guid> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new NotificationQueue
        {
            NotificationType = request.NotificationType,
            Recipient = request.Recipient,
            Subject = request.Subject ?? string.Empty,
            Body = request.Body,
            TemplateKey = request.TemplateKey,
            TemplateData = request.TemplateData,
            Priority = request.Priority,
            Status = request.ScheduledAt.HasValue ? NotificationStatus.Scheduled : NotificationStatus.Pending,
            NextRetryAt = request.ScheduledAt,
            MaxRetries = 3
        };

        return await _queueRepository.EnqueueAsync(notification, cancellationToken);
    }
}
