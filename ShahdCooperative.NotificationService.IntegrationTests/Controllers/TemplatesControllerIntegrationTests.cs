using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ShahdCooperative.NotificationService.Application.Commands.Template;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class TemplatesControllerIntegrationTests : IntegrationTestBase 
{

    public TemplatesControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }



    [Fact]
    public async Task GetAllTemplates_ReturnsOkWithListOfTemplates()
    {
        // Act
        var response = await Client.GetAsync("/api/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var templates = await response.Content.ReadFromJsonAsync<List<NotificationTemplate>>();
        templates.Should().NotBeNull();
        // Just verify the endpoint works - don't assert empty since cleanup may be async
    }

    [Fact]
    public async Task CreateTemplate_WithValidData_ReturnsCreated()
    {
        // Arrange - Use unique key for this test
        var uniqueKey = $"welcome-email-{Guid.NewGuid()}";
        var command = new CreateTemplateCommand
        {
            TemplateKey = uniqueKey,
            TemplateName = "Welcome Email",
            Subject = "Welcome to our service!",
            BodyTemplate = "Hello {{name}}, welcome!",
            NotificationType = NotificationType.Email,
            IsActive = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/templates", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTemplateByKey_WithExistingKey_ReturnsTemplate()
    {
        // Arrange - Create a template first with unique key
        var uniqueKey = $"test-template-{Guid.NewGuid()}";
        var createCommand = new CreateTemplateCommand
        {
            TemplateKey = uniqueKey,
            TemplateName = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body {{variable}}",
            NotificationType = NotificationType.Email,
            IsActive = true
        };
        await Client.PostAsJsonAsync("/api/templates", createCommand);

        // Act
        var response = await Client.GetAsync($"/api/templates/{uniqueKey}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var template = await response.Content.ReadFromJsonAsync<NotificationTemplate>();
        template.Should().NotBeNull();
        template!.Key.Should().Be(uniqueKey);
        template.Name.Should().Be("Test Template");
    }

    [Fact]
    public async Task GetTemplateByKey_WithNonExistingKey_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/templates/non-existing-key");

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
        await Client.PostAsJsonAsync("/api/templates", createCommand);

        var updateCommand = new UpdateTemplateCommand
        {
            TemplateKey = "update-test",
            TemplateName = "Updated Name",
            Subject = "Updated Subject",
            BodyTemplate = "Updated Body {{variable}}",
            IsActive = true
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/templates/update-test", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await Client.GetAsync("/api/templates/update-test");
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
        var response = await Client.PutAsJsonAsync("/api/templates/different-key", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTemplate_WithExistingKey_ReturnsNoContent()
    {
        // Arrange - Create a template first with unique key
        var uniqueKey = $"delete-test-{Guid.NewGuid()}";
        var createCommand = new CreateTemplateCommand
        {
            TemplateKey = uniqueKey,
            TemplateName = "To Delete",
            Subject = "Subject",
            BodyTemplate = "Body",
            NotificationType = NotificationType.Email,
            IsActive = true
        };
        await Client.PostAsJsonAsync("/api/templates", createCommand);

        // Act
        var response = await Client.DeleteAsync($"/api/templates/{uniqueKey}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await Client.GetAsync($"/api/templates/{uniqueKey}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTemplate_WithNonExistingKey_ReturnsNotFound()
    {
        // Act
        var response = await Client.DeleteAsync("/api/templates/non-existing");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
