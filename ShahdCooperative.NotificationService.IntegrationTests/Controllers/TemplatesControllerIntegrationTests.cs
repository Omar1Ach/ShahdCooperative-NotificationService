using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ShahdCooperative.NotificationService.Application.Commands.Template;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

[Collection("Sequential")]
public class TemplatesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TemplatesControllerIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task GetAllTemplates_ReturnsEmptyList_WhenNoTemplates()
    {
        // Act
        var response = await _client.GetAsync("/api/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var templates = await response.Content.ReadFromJsonAsync<List<NotificationTemplate>>();
        templates.Should().NotBeNull();
        templates.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTemplate_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            TemplateKey = "welcome-email",
            TemplateName = "Welcome Email",
            Subject = "Welcome to our service!",
            BodyTemplate = "Hello {{name}}, welcome!",
            NotificationType = NotificationType.Email,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/templates", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTemplateByKey_WithExistingKey_ReturnsTemplate()
    {
        // Arrange - Create a template first
        var createCommand = new CreateTemplateCommand
        {
            TemplateKey = "test-template",
            TemplateName = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body {{variable}}",
            NotificationType = NotificationType.Email,
            IsActive = true
        };
        await _client.PostAsJsonAsync("/api/templates", createCommand);

        // Act
        var response = await _client.GetAsync("/api/templates/test-template");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var template = await response.Content.ReadFromJsonAsync<NotificationTemplate>();
        template.Should().NotBeNull();
        template!.Key.Should().Be("test-template");
        template.Name.Should().Be("Test Template");
    }

    [Fact]
    public async Task GetTemplateByKey_WithNonExistingKey_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/templates/non-existing-key");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTemplate_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create a template first
        var createCommand = new CreateTemplateCommand
        {
            TemplateKey = "update-test",
            TemplateName = "Original Name",
            Subject = "Original Subject",
            BodyTemplate = "Original Body",
            NotificationType = NotificationType.Email,
            IsActive = true
        };
        await _client.PostAsJsonAsync("/api/templates", createCommand);

        var updateCommand = new UpdateTemplateCommand
        {
            TemplateKey = "update-test",
            TemplateName = "Updated Name",
            Subject = "Updated Subject",
            BodyTemplate = "Updated Body {{variable}}",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/templates/update-test", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync("/api/templates/update-test");
        var template = await getResponse.Content.ReadFromJsonAsync<NotificationTemplate>();
        template!.Name.Should().Be("Updated Name");
        template.Subject.Should().Be("Updated Subject");
    }

    [Fact]
    public async Task UpdateTemplate_WithMismatchedKey_ReturnsBadRequest()
    {
        // Arrange
        var updateCommand = new UpdateTemplateCommand
        {
            TemplateKey = "key-in-body",
            TemplateName = "Test",
            Subject = "Test",
            BodyTemplate = "Test",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/templates/different-key", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTemplate_WithExistingKey_ReturnsNoContent()
    {
        // Arrange - Create a template first
        var createCommand = new CreateTemplateCommand
        {
            TemplateKey = "delete-test",
            TemplateName = "To Delete",
            Subject = "Subject",
            BodyTemplate = "Body",
            NotificationType = NotificationType.Email,
            IsActive = true
        };
        await _client.PostAsJsonAsync("/api/templates", createCommand);

        // Act
        var response = await _client.DeleteAsync("/api/templates/delete-test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync("/api/templates/delete-test");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTemplate_WithNonExistingKey_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/templates/non-existing");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
