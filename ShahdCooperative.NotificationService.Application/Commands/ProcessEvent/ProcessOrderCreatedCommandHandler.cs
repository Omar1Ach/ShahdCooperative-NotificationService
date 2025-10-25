using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessOrderCreatedCommandHandler : IRequestHandler<ProcessOrderCreatedCommand, bool>
{
    private readonly INotificationQueueRepository _queueRepository;

    public ProcessOrderCreatedCommandHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<bool> Handle(ProcessOrderCreatedCommand request, CancellationToken cancellationToken)
    {
        var templateData = new
        {
            OrderId = request.Event.OrderId.ToString(),
            request.Event.Email,
            TotalAmount = request.Event.TotalAmount.ToString("C"),
            request.Event.ItemCount,
            OrderDate = request.Event.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")
        };

        var notification = new NotificationQueue
        {
            NotificationType = NotificationType.Email,
            Recipient = request.Event.Email,
            Subject = "Order Confirmation - ShahdCooperative",
            TemplateKey = "order-created",
            TemplateData = JsonSerializer.Serialize(templateData),
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending,
            MaxRetries = 3
        };

        await _queueRepository.EnqueueAsync(notification, cancellationToken);
        return true;
    }
}
