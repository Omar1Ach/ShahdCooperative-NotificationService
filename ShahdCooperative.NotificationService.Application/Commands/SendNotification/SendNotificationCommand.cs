using MediatR;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Application.Commands.SendNotification;

public class SendNotificationCommand : IRequest<Guid>
{
    public NotificationType NotificationType { get; set; }
    public string Recipient { get; set; } = null!;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? TemplateKey { get; set; }
    public string? TemplateData { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public DateTime? ScheduledAt { get; set; }
}
