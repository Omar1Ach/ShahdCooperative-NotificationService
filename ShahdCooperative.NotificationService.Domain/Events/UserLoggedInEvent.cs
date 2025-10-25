namespace ShahdCooperative.NotificationService.Domain.Events;

public class UserLoggedInEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
}
