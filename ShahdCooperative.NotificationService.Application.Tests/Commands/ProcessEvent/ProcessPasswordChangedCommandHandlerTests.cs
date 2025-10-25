using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Events;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.ProcessEvent;

public class ProcessPasswordChangedCommandHandlerTests
{
    private readonly Mock<INotificationQueueRepository> _mockQueueRepository;
    private readonly ProcessPasswordChangedCommandHandler _handler;

    public ProcessPasswordChangedCommandHandlerTests()
    {
        _mockQueueRepository = new Mock<INotificationQueueRepository>();
        _handler = new ProcessPasswordChangedCommandHandler(_mockQueueRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Enqueue_Security_Alert_Email()
    {
        var command = new ProcessPasswordChangedCommand
        {
            Event = new PasswordChangedEvent
            {
                UserId = Guid.NewGuid(),
                Email = "user@example.com",
                IpAddress = "192.168.1.1",
                ChangedAt = DateTime.UtcNow
            }
        };

        NotificationQueue? capturedNotification = null;
        _mockQueueRepository.Setup(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationQueue, CancellationToken>((notification, _) => capturedNotification = notification)
            .ReturnsAsync(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockQueueRepository.Verify(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedNotification.Should().NotBeNull();
        capturedNotification!.NotificationType.Should().Be(NotificationType.Email);
        capturedNotification.Recipient.Should().Be("user@example.com");
        capturedNotification.Subject.Should().Contain("Password Changed");
        capturedNotification.TemplateKey.Should().Be("password-changed");
        capturedNotification.Priority.Should().Be(NotificationPriority.Highest);
    }
}
