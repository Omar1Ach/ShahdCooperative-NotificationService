namespace ShahdCooperative.NotificationService.Domain.Events;

public class OrderCreatedEvent : BaseEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public int ItemCount { get; set; }
}
