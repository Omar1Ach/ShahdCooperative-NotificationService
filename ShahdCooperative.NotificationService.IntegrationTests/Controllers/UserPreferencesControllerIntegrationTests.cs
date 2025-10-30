using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ShahdCooperative.NotificationService.Application.Commands.UserPreference;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

public class UserPreferencesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UserPreferencesControllerIntegrationTests(CustomWebApplicationFactory factory)
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

    [Fact]
    public async Task GetUserPreferences_WithNonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/userpreferences/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUserPreferences_WithValidData_ReturnsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateUserPreferenceCommand
        {
            UserId = userId,
            EmailNotifications = true,
            SmsNotifications = false,
            PushNotifications = true,
            InAppNotifications = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/userpreferences", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserPreferences_WithExistingUser_ReturnsPreferences()
    {
        // Arrange - Create preferences first
        var userId = Guid.NewGuid();
        var createCommand = new CreateUserPreferenceCommand
        {
            UserId = userId,
            EmailNotifications = true,
            SmsNotifications = false,
            PushNotifications = true,
            InAppNotifications = false
        };
        await _client.PostAsJsonAsync("/api/userpreferences", createCommand);

        // Act
        var response = await _client.GetAsync($"/api/userpreferences/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var preferences = await response.Content.ReadFromJsonAsync<NotificationPreference>();
        preferences.Should().NotBeNull();
        preferences!.UserId.Should().Be(userId);
        preferences.EmailNotifications.Should().BeTrue();
        preferences.SmsNotifications.Should().BeFalse();
        preferences.PushNotifications.Should().BeTrue();
        preferences.InAppNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserPreferences_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create preferences first
        var userId = Guid.NewGuid();
        var createCommand = new CreateUserPreferenceCommand
        {
            UserId = userId,
            EmailNotifications = true,
            SmsNotifications = true,
            PushNotifications = true,
            InAppNotifications = true
        };
        await _client.PostAsJsonAsync("/api/userpreferences", createCommand);

        var updateCommand = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailNotifications = false,
            SmsNotifications = false,
            PushNotifications = false,
            InAppNotifications = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/userpreferences/{userId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/userpreferences/{userId}");
        var preferences = await getResponse.Content.ReadFromJsonAsync<NotificationPreference>();
        preferences!.EmailNotifications.Should().BeFalse();
        preferences.SmsNotifications.Should().BeFalse();
        preferences.PushNotifications.Should().BeFalse();
        preferences.InAppNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserPreferences_WithMismatchedUserId_ReturnsBadRequest()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var updateCommand = new UpdateUserPreferenceCommand
        {
            UserId = userId1,
            EmailNotifications = true,
            SmsNotifications = true,
            PushNotifications = true,
            InAppNotifications = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/userpreferences/{userId2}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserPreferences_WithNonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateCommand = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailNotifications = true,
            SmsNotifications = true,
            PushNotifications = true,
            InAppNotifications = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/userpreferences/{userId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
