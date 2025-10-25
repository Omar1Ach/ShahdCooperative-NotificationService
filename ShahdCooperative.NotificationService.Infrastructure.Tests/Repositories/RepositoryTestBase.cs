using FluentAssertions;
using ShahdCooperative.NotificationService.Infrastructure.Repositories;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Repositories;

/// <summary>
/// Base class for repository tests with common test scenarios
/// </summary>
public class RepositoryTestBase
{
    protected readonly string ConnectionString = "Server=localhost;Database=Test;Integrated Security=true;";

    [Fact]
    public void DeviceTokenRepository_Should_Not_Accept_Null_ConnectionString()
    {
        // Act
        Action act = () => new DeviceTokenRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotificationLogRepository_Should_Not_Accept_Null_ConnectionString()
    {
        // Act
        Action act = () => new NotificationLogRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotificationPreferenceRepository_Should_Not_Accept_Null_ConnectionString()
    {
        // Act
        Action act = () => new NotificationPreferenceRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void All_Repositories_Should_Be_Creatable_With_Valid_ConnectionString()
    {
        // Act & Assert
        var templateRepo = new NotificationTemplateRepository(ConnectionString);
        var queueRepo = new NotificationQueueRepository(ConnectionString);
        var preferenceRepo = new NotificationPreferenceRepository(ConnectionString);
        var inAppRepo = new InAppNotificationRepository(ConnectionString);
        var deviceTokenRepo = new DeviceTokenRepository(ConnectionString);
        var logRepo = new NotificationLogRepository(ConnectionString);

        templateRepo.Should().NotBeNull();
        queueRepo.Should().NotBeNull();
        preferenceRepo.Should().NotBeNull();
        inAppRepo.Should().NotBeNull();
        deviceTokenRepo.Should().NotBeNull();
        logRepo.Should().NotBeNull();
    }

    [Fact]
    public void All_Repositories_Should_Implement_Their_Interfaces()
    {
        // Arrange & Act
        var templateRepo = new NotificationTemplateRepository(ConnectionString);
        var queueRepo = new NotificationQueueRepository(ConnectionString);
        var preferenceRepo = new NotificationPreferenceRepository(ConnectionString);
        var inAppRepo = new InAppNotificationRepository(ConnectionString);
        var deviceTokenRepo = new DeviceTokenRepository(ConnectionString);
        var logRepo = new NotificationLogRepository(ConnectionString);

        // Assert
        templateRepo.Should().BeAssignableTo<Domain.Interfaces.INotificationTemplateRepository>();
        queueRepo.Should().BeAssignableTo<Domain.Interfaces.INotificationQueueRepository>();
        preferenceRepo.Should().BeAssignableTo<Domain.Interfaces.INotificationPreferenceRepository>();
        inAppRepo.Should().BeAssignableTo<Domain.Interfaces.IInAppNotificationRepository>();
        deviceTokenRepo.Should().BeAssignableTo<Domain.Interfaces.IDeviceTokenRepository>();
        logRepo.Should().BeAssignableTo<Domain.Interfaces.INotificationLogRepository>();
    }
}
