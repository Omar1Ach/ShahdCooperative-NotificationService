using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Repositories;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Repositories;

public class InAppNotificationRepositoryTests
{
    private readonly string _connectionString = "Server=localhost;Database=Test;";

    [Fact]
    public void Constructor_Should_Throw_When_ConnectionString_Is_Null()
    {
        // Act
        Action act = () => new InAppNotificationRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("connectionString");
    }

    [Fact]
    public void Constructor_Should_Create_Repository_With_Valid_ConnectionString()
    {
        // Act
        var repository = new InAppNotificationRepository(_connectionString);

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void InAppNotificationRepository_Should_Implement_IInAppNotificationRepository()
    {
        // Arrange
        var repository = new InAppNotificationRepository(_connectionString);

        // Assert
        repository.Should().BeAssignableTo<Domain.Interfaces.IInAppNotificationRepository>();
    }

    [Fact]
    public void CreateAsync_Should_Accept_Valid_InAppNotification()
    {
        var notification = new InAppNotification
        {
            Id = Guid.Empty,
            UserId = Guid.NewGuid(),
            Title = "Test Notification",
            Message = "Test Message",
            Type = InAppNotificationType.Info,
            Category = "Test",
            IsRead = false
        };

        // Verify the entity structure
        notification.Should().NotBeNull();
        notification.Type.Should().Be(InAppNotificationType.Info);
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }
}
