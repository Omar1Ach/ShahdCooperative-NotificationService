using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ShahdCooperative.NotificationService.Application.Commands.SendNotification;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class NotificationsControllerIntegrationTests : IntegrationTestBase 
{

    public NotificationsControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }



    [Fact]
    public async Task SendNotification_WithValidEmailData_ReturnsOk()
    {
        // Arrange
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.Email,
            Recipient = "test@example.com",
            Subject = "Test Email",
            Body = "This is a test email",
            Priority = NotificationPriority.Highest
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result.Should().ContainKey("notificationId");
        result.Should().ContainKey("message");
    }

    [Fact]
    public async Task SendNotification_WithValidSmsData_ReturnsOk()
    {
        // Arrange
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.SMS,
            Recipient = "+1234567890",
            Body = "This is a test SMS",
            Priority = NotificationPriority.High
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result.Should().ContainKey("notificationId");
    }

    [Fact]
    public async Task SendNotification_WithValidPushData_ReturnsOk()
    {
        // Arrange
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.Push,
            Recipient = "device-token-123",
            Subject = "Test Push",
            Body = "This is a test push notification",
            Priority = NotificationPriority.Normal
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result.Should().ContainKey("notificationId");
    }

    [Fact]
    public async Task SendNotification_WithScheduledTime_ReturnsOk()
    {
        // Arrange
        var scheduledAt = DateTime.UtcNow.AddHours(2);
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.Email,
            Recipient = "scheduled@example.com",
            Subject = "Scheduled Email",
            Body = "This email is scheduled",
            ScheduledAt = scheduledAt,
            Priority = NotificationPriority.Normal
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
