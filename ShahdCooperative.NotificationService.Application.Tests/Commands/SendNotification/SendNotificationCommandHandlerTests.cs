using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.SendNotification;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.SendNotification;

public class SendNotificationCommandHandlerTests
{
    private readonly Mock<INotificationQueueRepository> _mockQueueRepository;
    private readonly SendNotificationCommandHandler _handler;

    public SendNotificationCommandHandlerTests()
    {
        _mockQueueRepository = new Mock<INotificationQueueRepository>();
        _handler = new SendNotificationCommandHandler(_mockQueueRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Enqueue_Notification_With_Pending_Status()
    {
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.Email,
            Recipient = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Priority = NotificationPriority.High
        };

        var expectedId = Guid.NewGuid();
        _mockQueueRepository.Setup(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        _mockQueueRepository.Verify(x => x.EnqueueAsync(It.Is<NotificationQueue>(n =>
            n.NotificationType == command.NotificationType &&
            n.Recipient == command.Recipient &&
            n.Subject == command.Subject &&
            n.Body == command.Body &&
            n.Priority == command.Priority &&
            n.Status == NotificationStatus.Pending &&
            n.MaxRetries == 3
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Enqueue_Notification_With_Template()
    {
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.Email,
            Recipient = "user@example.com",
            TemplateKey = "welcome-email",
            TemplateData = "{\"name\":\"John\"}",
            Priority = NotificationPriority.Normal
        };

        var expectedId = Guid.NewGuid();
        _mockQueueRepository.Setup(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        _mockQueueRepository.Verify(x => x.EnqueueAsync(It.Is<NotificationQueue>(n =>
            n.TemplateKey == command.TemplateKey &&
            n.TemplateData == command.TemplateData &&
            n.Status == NotificationStatus.Pending
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Enqueue_Scheduled_Notification()
    {
        var scheduledTime = DateTime.UtcNow.AddHours(2);
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.SMS,
            Recipient = "+1234567890",
            Body = "Scheduled SMS",
            ScheduledAt = scheduledTime,
            Priority = NotificationPriority.Normal
        };

        var expectedId = Guid.NewGuid();
        _mockQueueRepository.Setup(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        _mockQueueRepository.Verify(x => x.EnqueueAsync(It.Is<NotificationQueue>(n =>
            n.Status == NotificationStatus.Scheduled &&
            n.NextRetryAt == scheduledTime
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Enqueue_Notification_Without_Subject()
    {
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.SMS,
            Recipient = "+1234567890",
            Body = "SMS Body",
            Priority = NotificationPriority.Normal
        };

        var expectedId = Guid.NewGuid();
        _mockQueueRepository.Setup(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        _mockQueueRepository.Verify(x => x.EnqueueAsync(It.Is<NotificationQueue>(n =>
            n.Subject == string.Empty
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_QueueRepository_Is_Null()
    {
        var act = () => new SendNotificationCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
