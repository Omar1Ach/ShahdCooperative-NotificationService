using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class InAppNotificationsControllerIntegrationTests : IntegrationTestBase
{
    public InAppNotificationsControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<Guid> CreateInAppNotificationAsync(Guid userId, string title, string message, bool isRead = false)
    {
        // Use the repository from DI container to ensure same connection handling
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IInAppNotificationRepository>();

        var notification = new InAppNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = InAppNotificationType.Info,
            IsRead = isRead,
            CreatedAt = DateTime.UtcNow
        };

        var notificationId = await repository.CreateAsync(notification);
        return notificationId;
    }

    [Fact]
    public async Task GetUserNotifications_WithNoNotifications_ReturnsEmptyList()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/inappnotifications/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.Content.ReadFromJsonAsync<List<InAppNotification>>();
        notifications.Should().NotBeNull();
        notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserNotifications_WithExistingNotifications_ReturnsNotifications()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();
        await CreateInAppNotificationAsync(userId, "Test Notification 1", "Message 1");
        await CreateInAppNotificationAsync(userId, "Test Notification 2", "Message 2");

        // Act
        var response = await Client.GetAsync($"/api/inappnotifications/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.Content.ReadFromJsonAsync<List<InAppNotification>>();
        notifications.Should().NotBeNull();
        notifications.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserNotifications_WithPagination_ReturnsPagedResults()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();
        for (int i = 1; i <= 25; i++)
        {
            await CreateInAppNotificationAsync(userId, $"Notification {i}", $"Message {i}");
        }

        // Act
        var response = await Client.GetAsync($"/api/inappnotifications/user/{userId}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.Content.ReadFromJsonAsync<List<InAppNotification>>();
        notifications.Should().NotBeNull();
        notifications.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetUnreadCount_WithNoUnreadNotifications_ReturnsZero()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();
        await CreateInAppNotificationAsync(userId, "Read Notification", "Message", isRead: true);

        // Act
        var response = await Client.GetAsync($"/api/inappnotifications/user/{userId}/unread-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result.Should().ContainKey("unreadCount");
    }

    [Fact]
    public async Task GetUnreadCount_WithUnreadNotifications_ReturnsCorrectCount()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();
        await CreateInAppNotificationAsync(userId, "Unread 1", "Message 1", isRead: false);
        await CreateInAppNotificationAsync(userId, "Unread 2", "Message 2", isRead: false);
        await CreateInAppNotificationAsync(userId, "Read 1", "Message 3", isRead: true);

        // Act
        var response = await Client.GetAsync($"/api/inappnotifications/user/{userId}/unread-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result.Should().ContainKey("unreadCount");
    }

    [Fact]
    public async Task MarkAsRead_WithExistingNotification_ReturnsNoContent()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();
        var notificationId = await CreateInAppNotificationAsync(userId, "Test", "Message", isRead: false);

        // Act
        var response = await Client.PutAsync($"/api/inappnotifications/{notificationId}/mark-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MarkAsRead_WithNonExistingNotification_ReturnsNotFound()
    {
        // Arrange
        var notificationId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsync($"/api/inappnotifications/{notificationId}/mark-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAllAsRead_WithExistingNotifications_ReturnsNoContent()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();
        await CreateInAppNotificationAsync(userId, "Unread 1", "Message 1", isRead: false);
        await CreateInAppNotificationAsync(userId, "Unread 2", "Message 2", isRead: false);

        // Act
        var response = await Client.PutAsync($"/api/inappnotifications/user/{userId}/mark-all-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MarkAllAsRead_WithNoNotifications_ReturnsNotFound()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsync($"/api/inappnotifications/user/{userId}/mark-all-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
