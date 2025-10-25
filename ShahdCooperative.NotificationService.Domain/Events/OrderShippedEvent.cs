namespace ShahdCooperative.NotificationService.Domain.Events;

public class OrderShippedEvent : BaseEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string? Carrier { get; set; }
    public DateTime ShippedDate { get; set; } = DateTime.UtcNow;
    public DateTime? EstimatedDeliveryDate { get; set; }
}
