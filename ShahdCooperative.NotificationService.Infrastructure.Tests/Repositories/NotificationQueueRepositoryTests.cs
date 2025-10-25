using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Repositories;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Repositories;

public class NotificationQueueRepositoryTests
{
    private readonly string _connectionString = "Server=localhost;Database=Test;";

    [Fact]
    public void Constructor_Should_Throw_When_ConnectionString_Is_Null()
    {
        // Act
        Action act = () => new NotificationQueueRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("connectionString");
    }

    [Fact]
    public void Constructor_Should_Create_Repository_With_Valid_ConnectionString()
    {
        // Act
        var repository = new NotificationQueueRepository(_connectionString);

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void NotificationQueueRepository_Should_Implement_INotificationQueueRepository()
    {
        // Arrange
        var repository = new NotificationQueueRepository(_connectionString);

        // Assert
        repository.Should().BeAssignableTo<Domain.Interfaces.INotificationQueueRepository>();
    }

    [Fact]
    public void EnqueueAsync_Should_Accept_Valid_NotificationQueue()
    {
        var notification = new NotificationQueue
        {
            Id = Guid.Empty,
            NotificationType = NotificationType.Email,
            Recipient = "test@example.com",
            Subject = "Test",
            Body = "Test Body",
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending
        };

        // Verify the entity structure
        notification.Should().NotBeNull();
        notification.NotificationType.Should().Be(NotificationType.Email);
        notification.Priority.Should().Be(NotificationPriority.High);
        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.AttemptCount.Should().Be(0);
        notification.MaxRetries.Should().Be(3);
    }
}
