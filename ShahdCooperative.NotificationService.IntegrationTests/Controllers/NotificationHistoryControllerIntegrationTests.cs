using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class NotificationHistoryControllerIntegrationTests : IntegrationTestBase
{

    public NotificationHistoryControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }



    private async Task CreateNotificationLogAsync(string recipient, NotificationType notificationType, NotificationStatus status)
    {
        // Use the repository from DI container to ensure same connection handling
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationLogRepository>();

        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            RecipientEmail = recipient,
            Type = notificationType,
            Subject = "Test Subject",
            Message = "Test Body",
            Status = status,
            SentAt = DateTime.UtcNow,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await repository.CreateAsync(log);
    }

    [Fact]
    public async Task GetNotificationHistory_WithNoHistory_ReturnsEmptyList()
    {
        // Arrange - Use unique recipient for this test
        var recipient = $"test-{Guid.NewGuid()}@example.com";

        // Act
        var response = await Client.GetAsync($"/api/notificationhistory/{recipient}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<NotificationLog>>();
        history.Should().NotBeNull();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNotificationHistory_WithExistingHistory_ReturnsHistory()
    {
        // Arrange - Use unique recipient for this test
        var recipient = $"user-{Guid.NewGuid()}@example.com";
        await CreateNotificationLogAsync(recipient, NotificationType.Email, NotificationStatus.Sent);
        await CreateNotificationLogAsync(recipient, NotificationType.SMS, NotificationStatus.Sent);
        await CreateNotificationLogAsync(recipient, NotificationType.Push, NotificationStatus.Failed);

        // Act
        var response = await Client.GetAsync($"/api/notificationhistory/{recipient}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<NotificationLog>>();
        history.Should().NotBeNull();
        history.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetNotificationHistory_ReturnsOnlyRecipientHistory()
    {
        // Arrange - Use unique recipients for this test
        var recipient1 = $"user1-{Guid.NewGuid()}@example.com";
        var recipient2 = $"user2-{Guid.NewGuid()}@example.com";

        await CreateNotificationLogAsync(recipient1, NotificationType.Email, NotificationStatus.Sent);
        await CreateNotificationLogAsync(recipient1, NotificationType.SMS, NotificationStatus.Sent);
        await CreateNotificationLogAsync(recipient2, NotificationType.Email, NotificationStatus.Sent);

        // Act
        var response = await Client.GetAsync($"/api/notificationhistory/{recipient1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<NotificationLog>>();
        history.Should().NotBeNull();
        history.Should().HaveCount(2);
        history.Should().OnlyContain(log => log.RecipientEmail == recipient1);
    }
}
