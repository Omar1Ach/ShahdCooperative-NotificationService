namespace ShahdCooperative.NotificationService.Application.DTOs;

public class HealthStatusDto
{
    public string Status { get; set; } = null!;
    public bool IsDatabaseHealthy { get; set; }
    public bool IsQueueHealthy { get; set; }
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = null!;
}
