using Microsoft.AspNetCore.SignalR;

namespace ShahdCooperative.NotificationService.API.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        _logger.LogInformation("Client {ConnectionId} joined user group: {UserId}", Context.ConnectionId, userId);
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        _logger.LogInformation("Client {ConnectionId} left user group: {UserId}", Context.ConnectionId, userId);
    }

    public async Task MarkAsRead(Guid notificationId)
    {
        _logger.LogInformation("Notification marked as read: {NotificationId} by connection {ConnectionId}",
            notificationId, Context.ConnectionId);

        // Broadcast to all clients in the same user group that the notification was read
        await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
    }
}
