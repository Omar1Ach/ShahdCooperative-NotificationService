using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Services.InApp;

public class NotificationHubClient : INotificationHubClient
{
    private readonly object _hubContext;

    public NotificationHubClient(object hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task SendNotificationToUserAsync(string userId, object notification)
    {
        // Use reflection to call SignalR hub context
        var hubContextType = _hubContext.GetType();
        var clientsProperty = hubContextType.GetProperty("Clients");
        var clients = clientsProperty?.GetValue(_hubContext);

        if (clients != null)
        {
            var groupMethod = clients.GetType().GetMethod("Group");
            var groupClient = groupMethod?.Invoke(clients, new object[] { $"user_{userId}" });

            if (groupClient != null)
            {
                var sendAsyncMethod = groupClient.GetType().GetMethod("SendAsync");
                var task = sendAsyncMethod?.Invoke(groupClient, new object[] { "ReceiveNotification", notification, default(CancellationToken) }) as Task;
                if (task != null)
                {
                    await task;
                }
            }
        }
    }
}
