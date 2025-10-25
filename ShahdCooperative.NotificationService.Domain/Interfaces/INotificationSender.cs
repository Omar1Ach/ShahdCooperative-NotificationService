using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface INotificationSender
{
    NotificationType NotificationType { get; }
    Task<bool> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
}
