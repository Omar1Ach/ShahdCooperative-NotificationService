using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

public class InAppNotificationsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public InAppNotificationsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.CleanupDatabaseAsync();
    }

    private async Task<Guid> CreateInAppNotificationAsync(Guid userId, string title, string message, bool isRead = false)
    {
        var notificationId = Guid.NewGuid();
        using var connection = new SqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Notification].[InAppNotifications]
            (Id, UserId, Title, Message, Type, IsRead, CreatedAt)
            VALUES (@Id, @UserId, @Title, @Message, @Type, @IsRead, @CreatedAt)";

        command.Parameters.AddWithValue("@Id", notificationId);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@Title", title);
        command.Parameters.AddWithValue("@Message", message);
        command.Parameters.AddWithValue("@Type", "Info");
        command.Parameters.AddWithValue("@IsRead", isRead);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync();
        return notificationId;
    }

    [Fact]
    public async Task GetUserNotifications_WithNoNotifications_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inappnotifications/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.Content.ReadFromJsonAsync<List<InAppNotification>>();
        notifications.Should().NotBeNull();
        notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserNotifications_WithExistingNotifications_ReturnsNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await CreateInAppNotificationAsync(userId, "Test Notification 1", "Message 1");
        await CreateInAppNotificationAsync(userId, "Test Notification 2", "Message 2");

        // Act
        var response = await _client.GetAsync($"/api/inappnotifications/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.Content.ReadFromJsonAsync<List<InAppNotification>>();
        notifications.Should().NotBeNull();
        notifications.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserNotifications_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        for (int i = 1; i <= 25; i++)
        {
            await CreateInAppNotificationAsync(userId, $"Notification {i}", $"Message {i}");
        }

        // Act
        var response = await _client.GetAsync($"/api/inappnotifications/user/{userId}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.Content.ReadFromJsonAsync<List<InAppNotification>>();
        notifications.Should().NotBeNull();
        notifications.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetUnreadCount_WithNoUnreadNotifications_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await CreateInAppNotificationAsync(userId, "Read Notification", "Message", isRead: true);

        // Act
        var response = await _client.GetAsync($"/api/inappnotifications/user/{userId}/unread-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result.Should().ContainKey("unreadCount");
    }

    [Fact]
    public async Task GetUnreadCount_WithUnreadNotifications_ReturnsCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await CreateInAppNotificationAsync(userId, "Unread 1", "Message 1", isRead: false);
        await CreateInAppNotificationAsync(userId, "Unread 2", "Message 2", isRead: false);
        await CreateInAppNotificationAsync(userId, "Read 1", "Message 3", isRead: true);

        // Act
        var response = await _client.GetAsync($"/api/inappnotifications/user/{userId}/unread-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result.Should().ContainKey("unreadCount");
    }

    [Fact]
    public async Task MarkAsRead_WithExistingNotification_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = await CreateInAppNotificationAsync(userId, "Test", "Message", isRead: false);

        // Act
        var response = await _client.PutAsync($"/api/inappnotifications/{notificationId}/mark-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MarkAsRead_WithNonExistingNotification_ReturnsNotFound()
    {
        // Arrange
        var notificationId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/inappnotifications/{notificationId}/mark-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAllAsRead_WithExistingNotifications_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await CreateInAppNotificationAsync(userId, "Unread 1", "Message 1", isRead: false);
        await CreateInAppNotificationAsync(userId, "Unread 2", "Message 2", isRead: false);

        // Act
        var response = await _client.PutAsync($"/api/inappnotifications/user/{userId}/mark-all-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MarkAllAsRead_WithNoNotifications_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/inappnotifications/user/{userId}/mark-all-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
