using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ShahdCooperative.NotificationService.Application.Commands.UserPreference;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class UserPreferencesControllerIntegrationTests : IntegrationTestBase 
{

    public UserPreferencesControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }



    [Fact]
    public async Task GetUserPreferences_WithNonExistingUser_ReturnsNotFound()
    {
        // Arrange - Use a user ID that doesn't exist in the database
        var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var response = await Client.GetAsync($"/api/userpreferences/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUserPreferences_WithValidData_ReturnsCreated()
    {
        // Arrange - Use unique user ID for this test
        var userId = Guid.NewGuid();
        var command = new CreateUserPreferenceCommand
        {
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = false,
            PushEnabled = true,
            InAppEnabled = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/userpreferences", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserPreferences_WithExistingUser_ReturnsPreferences()
    {
        // Arrange - Create preferences first with unique user ID
        var userId = Guid.NewGuid();
        var createCommand = new CreateUserPreferenceCommand
        {
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = false,
            PushEnabled = true,
            InAppEnabled = false
        };
        await Client.PostAsJsonAsync("/api/userpreferences", createCommand);

        // Act
        var response = await Client.GetAsync($"/api/userpreferences/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var preferences = await response.Content.ReadFromJsonAsync<NotificationPreference>();
        preferences.Should().NotBeNull();
        preferences!.UserId.Should().Be(userId);
        preferences.EmailEnabled.Should().BeTrue();
        preferences.SmsEnabled.Should().BeFalse();
        preferences.PushEnabled.Should().BeTrue();
        preferences.InAppEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserPreferences_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create preferences first with unique user ID
        var userId = Guid.NewGuid();
        var createCommand = new CreateUserPreferenceCommand
        {
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = true,
            PushEnabled = true,
            InAppEnabled = true
        };
        await Client.PostAsJsonAsync("/api/userpreferences", createCommand);

        var updateCommand = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailEnabled = false,
            SmsEnabled = false,
            PushEnabled = false,
            InAppEnabled = false
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/userpreferences/{userId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await Client.GetAsync($"/api/userpreferences/{userId}");
        var preferences = await getResponse.Content.ReadFromJsonAsync<NotificationPreference>();
        preferences!.EmailEnabled.Should().BeFalse();
        preferences.SmsEnabled.Should().BeFalse();
        preferences.PushEnabled.Should().BeFalse();
        preferences.InAppEnabled.Should().BeFalse();
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
            EmailEnabled = true,
            SmsEnabled = true,
            PushEnabled = true,
            InAppEnabled = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/userpreferences/{userId2}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserPreferences_WithNonExistingUser_ReturnsNotFound()
    {
        // Arrange - Use a user ID that doesn't exist in the database
        var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var updateCommand = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = true,
            PushEnabled = true,
            InAppEnabled = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/userpreferences/{userId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
