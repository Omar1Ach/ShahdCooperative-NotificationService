using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessUserLoggedInCommandHandler : IRequestHandler<ProcessUserLoggedInCommand, bool>
{
    private readonly INotificationQueueRepository _queueRepository;

    public ProcessUserLoggedInCommandHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<bool> Handle(ProcessUserLoggedInCommand request, CancellationToken cancellationToken)
    {
        var templateData = new
        {
            request.Event.Email,
            request.Event.IpAddress,
            request.Event.UserAgent,
            LoginTime = request.Event.LoginTime.ToString("yyyy-MM-dd HH:mm:ss UTC")
        };

        var notification = new NotificationQueue
        {
            NotificationType = NotificationType.Email,
            Recipient = request.Event.Email,
            Subject = "New Login Detected - ShahdCooperative",
            TemplateKey = "user-logged-in",
            TemplateData = JsonSerializer.Serialize(templateData),
            Priority = NotificationPriority.Normal,
            Status = NotificationStatus.Pending,
            MaxRetries = 3
        };

        await _queueRepository.EnqueueAsync(notification, cancellationToken);
        return true;
    }
}
