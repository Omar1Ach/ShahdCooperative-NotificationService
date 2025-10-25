using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessFeedbackReceivedCommandHandler : IRequestHandler<ProcessFeedbackReceivedCommand, bool>
{
    private readonly INotificationQueueRepository _queueRepository;

    public ProcessFeedbackReceivedCommandHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<bool> Handle(ProcessFeedbackReceivedCommand request, CancellationToken cancellationToken)
    {
        var templateData = new
        {
            request.Event.CustomerName,
            request.Event.CustomerEmail,
            request.Event.Rating,
            request.Event.Message,
            request.Event.Category
        };

        var notification = new NotificationQueue
        {
            NotificationType = NotificationType.Email,
            Recipient = request.Event.CustomerEmail,
            Subject = "Thank You for Your Feedback - ShahdCooperative",
            TemplateKey = "feedback-received",
            TemplateData = JsonSerializer.Serialize(templateData),
            Priority = NotificationPriority.Normal,
            Status = NotificationStatus.Pending,
            MaxRetries = 3
        };

        await _queueRepository.EnqueueAsync(notification, cancellationToken);
        return true;
    }
}
