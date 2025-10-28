using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Jobs;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Jobs;

public class ScheduledNotificationJobTests
{
    private readonly Mock<ILogger<ScheduledNotificationJob>> _mockLogger;
    private readonly Mock<INotificationQueueRepository> _mockQueueRepository;
    private readonly Mock<IInAppNotificationRepository> _mockInAppRepository;
    private readonly ServiceProvider _serviceProvider;

    public ScheduledNotificationJobTests()
    {
        _mockLogger = new Mock<ILogger<ScheduledNotificationJob>>();
        _mockQueueRepository = new Mock<INotificationQueueRepository>();
        _mockInAppRepository = new Mock<IInAppNotificationRepository>();

        var services = new ServiceCollection();
        services.AddScoped(_ => _mockQueueRepository.Object);
        services.AddScoped(_ => _mockInAppRepository.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new ScheduledNotificationJob(null!, _serviceProvider);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_ServiceProvider_Is_Null()
    {
        var act = () => new ScheduledNotificationJob(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessScheduledNotificationsAsync_Should_Not_Process_When_No_Scheduled_Notifications()
    {
        _mockQueueRepository.Setup(x => x.GetScheduledNotificationsAsync(
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue>());

        var job = new ScheduledNotificationJob(_mockLogger.Object, _serviceProvider);

        await job.ProcessScheduledNotificationsAsync();

        _mockQueueRepository.Verify(x => x.GetScheduledNotificationsAsync(
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockQueueRepository.Verify(x => x.UpdateStatusAsync(
            It.IsAny<Guid>(), It.IsAny<NotificationStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessScheduledNotificationsAsync_Should_Process_Scheduled_Notifications()
    {
        var notification = new NotificationQueue
        {
            Id = Guid.NewGuid(),
            Status = NotificationStatus.Scheduled,
            NotificationType = NotificationType.Email,
            Recipient = "test@example.com",
            Subject = "Test",
            Body = "Test Body"
        };

        _mockQueueRepository.Setup(x => x.GetScheduledNotificationsAsync(
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue> { notification });

        _mockQueueRepository.Setup(x => x.UpdateStatusAsync(
                It.IsAny<Guid>(), It.IsAny<NotificationStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var job = new ScheduledNotificationJob(_mockLogger.Object, _serviceProvider);

        await job.ProcessScheduledNotificationsAsync();

        _mockQueueRepository.Verify(x => x.UpdateStatusAsync(
            notification.Id, NotificationStatus.Pending, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CleanupExpiredNotificationsAsync_Should_Delete_Expired_Notifications()
    {
        _mockInAppRepository.Setup(x => x.DeleteExpiredNotificationsAsync(
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var job = new ScheduledNotificationJob(_mockLogger.Object, _serviceProvider);

        await job.CleanupExpiredNotificationsAsync();

        _mockInAppRepository.Verify(x => x.DeleteExpiredNotificationsAsync(
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RetryFailedNotificationsAsync_Should_Not_Process_When_No_Failed_Notifications()
    {
        _mockQueueRepository.Setup(x => x.GetFailedNotificationsForRetryAsync(
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue>());

        var job = new ScheduledNotificationJob(_mockLogger.Object, _serviceProvider);

        await job.RetryFailedNotificationsAsync();

        _mockQueueRepository.Verify(x => x.GetFailedNotificationsForRetryAsync(
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockQueueRepository.Verify(x => x.UpdateStatusAsync(
            It.IsAny<Guid>(), It.IsAny<NotificationStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RetryFailedNotificationsAsync_Should_Process_Failed_Notifications()
    {
        var notification = new NotificationQueue
        {
            Id = Guid.NewGuid(),
            Status = NotificationStatus.Failed,
            NotificationType = NotificationType.Email,
            Recipient = "test@example.com",
            Subject = "Test",
            Body = "Test Body",
            AttemptCount = 1,
            MaxRetries = 3
        };

        _mockQueueRepository.Setup(x => x.GetFailedNotificationsForRetryAsync(
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue> { notification });

        _mockQueueRepository.Setup(x => x.UpdateStatusAsync(
                It.IsAny<Guid>(), It.IsAny<NotificationStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var job = new ScheduledNotificationJob(_mockLogger.Object, _serviceProvider);

        await job.RetryFailedNotificationsAsync();

        _mockQueueRepository.Verify(x => x.UpdateStatusAsync(
            notification.Id, NotificationStatus.Pending, It.IsAny<CancellationToken>()), Times.Once);
    }
}
