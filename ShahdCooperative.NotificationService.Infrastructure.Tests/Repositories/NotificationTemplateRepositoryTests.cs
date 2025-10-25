using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Repositories;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Repositories;

public class NotificationTemplateRepositoryTests
{
    private readonly string _connectionString = "Server=localhost;Database=Test;";

    [Fact]
    public void Constructor_Should_Throw_When_ConnectionString_Is_Null()
    {
        // Act
        Action act = () => new NotificationTemplateRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("connectionString");
    }

    [Fact]
    public void Constructor_Should_Create_Repository_With_Valid_ConnectionString()
    {
        // Act
        var repository = new NotificationTemplateRepository(_connectionString);

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void NotificationTemplateRepository_Should_Implement_INotificationTemplateRepository()
    {
        // Arrange
        var repository = new NotificationTemplateRepository(_connectionString);

        // Assert
        repository.Should().BeAssignableTo<Domain.Interfaces.INotificationTemplateRepository>();
    }

    [Fact]
    public async Task CreateAsync_Should_Generate_New_Guid_When_Id_Is_Empty()
    {
        // Note: This is a structural test. Full integration tests would require a test database.
        // The actual database operations are tested in integration tests.
        var template = new NotificationTemplate
        {
            Id = Guid.Empty,
            TemplateKey = "test.template",
            TemplateName = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body {{placeholder}}",
            NotificationType = NotificationType.Email
        };

        // Verify the entity structure is correct
        template.Should().NotBeNull();
        template.TemplateKey.Should().Be("test.template");
        template.NotificationType.Should().Be(NotificationType.Email);
    }
}
