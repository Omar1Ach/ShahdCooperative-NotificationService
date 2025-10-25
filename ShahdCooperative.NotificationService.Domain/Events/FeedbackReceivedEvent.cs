namespace ShahdCooperative.NotificationService.Domain.Events;

public class FeedbackReceivedEvent : BaseEvent
{
    public Guid FeedbackId { get; set; }
    public Guid UserId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Category { get; set; }
}
