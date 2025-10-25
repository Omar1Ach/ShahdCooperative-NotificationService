namespace ShahdCooperative.NotificationService.Domain.Events;

public class UserRegisteredEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
