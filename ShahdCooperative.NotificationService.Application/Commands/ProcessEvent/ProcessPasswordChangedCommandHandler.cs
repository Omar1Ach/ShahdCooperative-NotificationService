using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessPasswordChangedCommandHandler : IRequestHandler<ProcessPasswordChangedCommand, bool>
{
    private readonly INotificationQueueRepository _queueRepository;

    public ProcessPasswordChangedCommandHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<bool> Handle(ProcessPasswordChangedCommand request, CancellationToken cancellationToken)
    {
        var templateData = new
        {
            request.Event.Email,
            request.Event.IpAddress,
            ChangedAt = request.Event.ChangedAt.ToString("yyyy-MM-dd HH:mm:ss UTC")
        };

        var notification = new NotificationQueue
        {
            NotificationType = NotificationType.Email,
            Recipient = request.Event.Email,
            Subject = "Password Changed - ShahdCooperative",
            TemplateKey = "password-changed",
            TemplateData = JsonSerializer.Serialize(templateData),
            Priority = NotificationPriority.Highest,
            Status = NotificationStatus.Pending,
            MaxRetries = 3
        };

        await _queueRepository.EnqueueAsync(notification, cancellationToken);
        return true;
    }
}
