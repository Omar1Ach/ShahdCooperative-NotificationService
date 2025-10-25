using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessUserRegisteredCommandHandler : IRequestHandler<ProcessUserRegisteredCommand, bool>
{
    private readonly INotificationQueueRepository _queueRepository;

    public ProcessUserRegisteredCommandHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<bool> Handle(ProcessUserRegisteredCommand request, CancellationToken cancellationToken)
    {
        var templateData = new
        {
            request.Event.FullName,
            request.Event.Email,
            UserId = request.Event.UserId.ToString()
        };

        var notification = new NotificationQueue
        {
            NotificationType = NotificationType.Email,
            Recipient = request.Event.Email,
            Subject = "Welcome to ShahdCooperative!",
            TemplateKey = "user-registered",
            TemplateData = JsonSerializer.Serialize(templateData),
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending,
            MaxRetries = 3
        };

        await _queueRepository.EnqueueAsync(notification, cancellationToken);
        return true;
    }
}
