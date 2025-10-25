using Microsoft.Extensions.Logging;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Services;

public class MockNotificationSender : INotificationSender
{
    private readonly ILogger<MockNotificationSender> _logger;

    public NotificationType NotificationType { get; }

    public MockNotificationSender(ILogger<MockNotificationSender> logger, NotificationType notificationType)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        NotificationType = notificationType;
    }

    public Task<bool> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK: Sending {NotificationType} to {Recipient} - Subject: {Subject}",
            NotificationType, recipient, subject);

        // Simulate successful send for now
        return Task.FromResult(true);
    }
}
