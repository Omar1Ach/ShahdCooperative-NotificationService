using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;

public class ProcessProductOutOfStockCommandHandler : IRequestHandler<ProcessProductOutOfStockCommand, bool>
{
    private readonly INotificationQueueRepository _queueRepository;

    public ProcessProductOutOfStockCommandHandler(INotificationQueueRepository queueRepository)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
    }

    public async Task<bool> Handle(ProcessProductOutOfStockCommand request, CancellationToken cancellationToken)
    {
        foreach (var email in request.Event.SubscriberEmails)
        {
            var templateData = new
            {
                request.Event.ProductName,
                request.Event.ProductSku
            };

            var notification = new NotificationQueue
            {
                NotificationType = NotificationType.Email,
                Recipient = email,
                Subject = "Product Back in Stock - ShahdCooperative",
                TemplateKey = "product-out-of-stock",
                TemplateData = JsonSerializer.Serialize(templateData),
                Priority = NotificationPriority.Low,
                Status = NotificationStatus.Pending,
                MaxRetries = 3
            };

            await _queueRepository.EnqueueAsync(notification, cancellationToken);
        }

        return true;
    }
}
