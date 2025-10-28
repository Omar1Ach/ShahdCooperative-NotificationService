using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Services;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services;

public class NotificationQueueProcessorTests
{
    private readonly Mock<ILogger<NotificationQueueProcessor>> _mockLogger;
    private readonly Mock<INotificationQueueRepository> _mockQueueRepository;
    private readonly Mock<INotificationLogRepository> _mockLogRepository;
    private readonly Mock<INotificationSender> _mockSender;
    private readonly Mock<ITemplateEngine> _mockTemplateEngine;
    private readonly NotificationSettings _settings;
    private readonly ServiceProvider _serviceProvider;

    public NotificationQueueProcessorTests()
    {
        _mockLogger = new Mock<ILogger<NotificationQueueProcessor>>();
        _mockQueueRepository = new Mock<INotificationQueueRepository>();
        _mockLogRepository = new Mock<INotificationLogRepository>();
        _mockSender = new Mock<INotificationSender>();
        _mockTemplateEngine = new Mock<ITemplateEngine>();

        _settings = new NotificationSettings
        {
            BatchSize = 10,
            MaxRetries = 3,
            RetryDelayMinutes = 5,
            ProcessingIntervalSeconds = 1
        };

        _mockSender.Setup(s => s.NotificationType).Returns(NotificationType.Email);
        _mockTemplateEngine.Setup(t => t.ProcessTemplateAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Processed template body");

        var services = new ServiceCollection();
        services.AddScoped(_ => _mockQueueRepository.Object);
        services.AddScoped(_ => _mockLogRepository.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new NotificationQueueProcessor(
            null!,
            Options.Create(_settings),
            _serviceProvider,
            new[] { _mockSender.Object },
            _mockTemplateEngine.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Is_Null()
    {
        var act = () => new NotificationQueueProcessor(
            _mockLogger.Object,
            null!,
            _serviceProvider,
            new[] { _mockSender.Object },
            _mockTemplateEngine.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_ServiceProvider_Is_Null()
    {
        var act = () => new NotificationQueueProcessor(
            _mockLogger.Object,
            Options.Create(_settings),
            null!,
            new[] { _mockSender.Object },
            _mockTemplateEngine.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Senders_Is_Null()
    {
        var act = () => new NotificationQueueProcessor(
            _mockLogger.Object,
            Options.Create(_settings),
            _serviceProvider,
            null!,
            _mockTemplateEngine.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_Should_Not_Process_When_No_Pending_Notifications()
    {
        _mockQueueRepository.Setup(x => x.GetPendingNotificationsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue>());

        var processor = new NotificationQueueProcessor(
            _mockLogger.Object,
            Options.Create(_settings),
            _serviceProvider,
            new[] { _mockSender.Object },
            _mockTemplateEngine.Object);

        // We can't directly test ProcessPendingNotificationsAsync as it's private,
        // but we can verify that GetPendingNotificationsAsync was called
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var task = processor.StartAsync(cts.Token);
        await Task.Delay(500);
        await processor.StopAsync(CancellationToken.None);

        _mockQueueRepository.Verify(x => x.GetPendingNotificationsAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_Process_Pending_Notification_Successfully()
    {
        var notificationId = Guid.NewGuid();
        var notification = new NotificationQueue
        {
            Id = notificationId,
            NotificationType = NotificationType.Email,
            Recipient = "test@example.com",
            Subject = "Test",
            Body = "Test Body",
            Status = NotificationStatus.Pending,
            AttemptCount = 0,
            MaxRetries = 3
        };

        _mockQueueRepository.Setup(x => x.GetPendingNotificationsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue> { notification });

        _mockSender.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockQueueRepository.Setup(x => x.UpdateStatusAsync(
                It.IsAny<Guid>(), It.IsAny<NotificationStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockLogRepository.Setup(x => x.CreateAsync(
                It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new NotificationQueueProcessor(
            _mockLogger.Object,
            Options.Create(_settings),
            _serviceProvider,
            new[] { _mockSender.Object },
            _mockTemplateEngine.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await processor.StartAsync(cts.Token);
        await Task.Delay(500);
        await processor.StopAsync(CancellationToken.None);

        _mockSender.Verify(s => s.SendAsync(
            "test@example.com", "Test", "Test Body", It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        _mockQueueRepository.Verify(x => x.UpdateStatusAsync(
            notificationId, NotificationStatus.Sent, It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        _mockLogRepository.Verify(x => x.CreateAsync(
            It.Is<NotificationLog>(log =>
                log.Type == NotificationType.Email &&
                log.Recipient == "test@example.com" &&
                log.Status == NotificationStatus.Sent),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_Handle_Failed_Notification_With_Retry()
    {
        var notificationId = Guid.NewGuid();
        var notification = new NotificationQueue
        {
            Id = notificationId,
            NotificationType = NotificationType.Email,
            Recipient = "test@example.com",
            Subject = "Test",
            Body = "Test Body",
            Status = NotificationStatus.Pending,
            AttemptCount = 0,
            MaxRetries = 3
        };

        _mockQueueRepository.Setup(x => x.GetPendingNotificationsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue> { notification });

        _mockSender.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockQueueRepository.Setup(x => x.UpdateStatusAsync(
                It.IsAny<Guid>(), It.IsAny<NotificationStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockQueueRepository.Setup(x => x.IncrementAttemptAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockQueueRepository.Setup(x => x.SetNextRetryAsync(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var processor = new NotificationQueueProcessor(
            _mockLogger.Object,
            Options.Create(_settings),
            _serviceProvider,
            new[] { _mockSender.Object },
            _mockTemplateEngine.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await processor.StartAsync(cts.Token);
        await Task.Delay(500);
        await processor.StopAsync(CancellationToken.None);

        _mockQueueRepository.Verify(x => x.IncrementAttemptAsync(
            notificationId, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        _mockQueueRepository.Verify(x => x.SetNextRetryAsync(
            notificationId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_Mark_As_Failed_When_No_Sender_Available()
    {
        var notificationId = Guid.NewGuid();
        var notification = new NotificationQueue
        {
            Id = notificationId,
            NotificationType = NotificationType.SMS, // SMS type, but only Email sender provided
            Recipient = "test@example.com",
            Subject = "Test",
            Body = "Test Body",
            Status = NotificationStatus.Pending,
            AttemptCount = 0,
            MaxRetries = 3
        };

        _mockQueueRepository.Setup(x => x.GetPendingNotificationsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue> { notification });

        _mockQueueRepository.Setup(x => x.UpdateStatusAsync(
                It.IsAny<Guid>(), It.IsAny<NotificationStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockQueueRepository.Setup(x => x.IncrementAttemptAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var processor = new NotificationQueueProcessor(
            _mockLogger.Object,
            Options.Create(_settings),
            _serviceProvider,
            new[] { _mockSender.Object },
            _mockTemplateEngine.Object); // Only Email sender

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await processor.StartAsync(cts.Token);
        await Task.Delay(500);
        await processor.StopAsync(CancellationToken.None);

        _mockQueueRepository.Verify(x => x.UpdateStatusAsync(
            notificationId, NotificationStatus.Failed, It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        _mockQueueRepository.Verify(x => x.IncrementAttemptAsync(
            notificationId, "No sender available", It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
