namespace ShahdCooperative.NotificationService.Domain.Events;

public class ProductOutOfStockEvent : BaseEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public List<string> SubscriberEmails { get; set; } = new();
}
