using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

public class NotificationHistoryControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public NotificationHistoryControllerIntegrationTests(CustomWebApplicationFactory factory)
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

    private async Task CreateNotificationLogAsync(string recipient, string notificationType, string status)
    {
        using var connection = new SqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Notification].[NotificationLogs]
            (Id, NotificationType, Recipient, Subject, Body, Status, SentAt, CreatedAt)
            VALUES (@Id, @NotificationType, @Recipient, @Subject, @Body, @Status, @SentAt, @CreatedAt)";

        command.Parameters.AddWithValue("@Id", Guid.NewGuid());
        command.Parameters.AddWithValue("@NotificationType", notificationType);
        command.Parameters.AddWithValue("@Recipient", recipient);
        command.Parameters.AddWithValue("@Subject", "Test Subject");
        command.Parameters.AddWithValue("@Body", "Test Body");
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@SentAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task GetNotificationHistory_WithNoHistory_ReturnsEmptyList()
    {
        // Arrange
        var recipient = "test@example.com";

        // Act
        var response = await _client.GetAsync($"/api/notificationhistory/{recipient}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<NotificationLog>>();
        history.Should().NotBeNull();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNotificationHistory_WithExistingHistory_ReturnsHistory()
    {
        // Arrange
        var recipient = "user@example.com";
        await CreateNotificationLogAsync(recipient, "Email", "Sent");
        await CreateNotificationLogAsync(recipient, "SMS", "Sent");
        await CreateNotificationLogAsync(recipient, "Push", "Failed");

        // Act
        var response = await _client.GetAsync($"/api/notificationhistory/{recipient}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<NotificationLog>>();
        history.Should().NotBeNull();
        history.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetNotificationHistory_ReturnsOnlyRecipientHistory()
    {
        // Arrange
        var recipient1 = "user1@example.com";
        var recipient2 = "user2@example.com";

        await CreateNotificationLogAsync(recipient1, "Email", "Sent");
        await CreateNotificationLogAsync(recipient1, "SMS", "Sent");
        await CreateNotificationLogAsync(recipient2, "Email", "Sent");

        // Act
        var response = await _client.GetAsync($"/api/notificationhistory/{recipient1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<NotificationLog>>();
        history.Should().NotBeNull();
        history.Should().HaveCount(2);
        history.Should().OnlyContain(log => log.Recipient == recipient1);
    }
}
