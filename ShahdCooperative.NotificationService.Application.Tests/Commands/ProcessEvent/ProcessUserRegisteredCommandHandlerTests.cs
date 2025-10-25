using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Events;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.ProcessEvent;

public class ProcessUserRegisteredCommandHandlerTests
{
    private readonly Mock<INotificationQueueRepository> _mockQueueRepository;
    private readonly ProcessUserRegisteredCommandHandler _handler;

    public ProcessUserRegisteredCommandHandlerTests()
    {
        _mockQueueRepository = new Mock<INotificationQueueRepository>();
        _handler = new ProcessUserRegisteredCommandHandler(_mockQueueRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Enqueue_Welcome_Email_Notification()
    {
        var command = new ProcessUserRegisteredCommand
        {
            Event = new UserRegisteredEvent
            {
                UserId = Guid.NewGuid(),
                Email = "newuser@example.com",
                FullName = "New User"
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
        capturedNotification.Recipient.Should().Be("newuser@example.com");
        capturedNotification.Subject.Should().Contain("Welcome");
        capturedNotification.TemplateKey.Should().Be("user-registered");
        capturedNotification.Priority.Should().Be(NotificationPriority.High);
        capturedNotification.Status.Should().Be(NotificationStatus.Pending);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_QueueRepository_Is_Null()
    {
        var act = () => new ProcessUserRegisteredCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
