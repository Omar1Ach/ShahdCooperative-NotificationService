using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessOrderShippedCommandHandler : IRequestHandler<ProcessOrderShippedCommand, bool>
{
    private readonly INotificationQueueRepository _queueRepository;

    public ProcessOrderShippedCommandHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<bool> Handle(ProcessOrderShippedCommand request, CancellationToken cancellationToken)
    {
        var templateData = new
        {
            OrderId = request.Event.OrderId.ToString(),
            request.Event.Email,
            request.Event.TrackingNumber,
            request.Event.Carrier,
            ShippedDate = request.Event.ShippedDate.ToString("yyyy-MM-dd"),
            EstimatedDelivery = request.Event.EstimatedDeliveryDate?.ToString("yyyy-MM-dd") ?? "TBD"
        };

        var notification = new NotificationQueue
        {
            NotificationType = NotificationType.Email,
            Recipient = request.Event.Email,
            Subject = "Your Order Has Shipped - ShahdCooperative",
            TemplateKey = "order-shipped",
            TemplateData = JsonSerializer.Serialize(templateData),
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending,
            MaxRetries = 3
        };

        await _queueRepository.EnqueueAsync(notification, cancellationToken);
        return true;
    }
}
