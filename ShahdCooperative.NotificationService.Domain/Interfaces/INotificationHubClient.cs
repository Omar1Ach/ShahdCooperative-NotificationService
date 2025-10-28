namespace ShahdCooperative.NotificationService.Domain.Interfaces;

public interface INotificationHubClient
{
    Task SendNotificationToUserAsync(string userId, object notification);
}
