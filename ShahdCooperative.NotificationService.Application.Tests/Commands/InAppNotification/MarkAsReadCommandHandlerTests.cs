using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.InAppNotification;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.InAppNotification;

public class MarkAsReadCommandHandlerTests
{
    private readonly Mock<IInAppNotificationRepository> _mockInAppRepository;
    private readonly MarkAsReadCommandHandler _handler;

    public MarkAsReadCommandHandlerTests()
    {
        _mockInAppRepository = new Mock<IInAppNotificationRepository>();
        _handler = new MarkAsReadCommandHandler(_mockInAppRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Mark_Notification_As_Read_And_Return_True()
    {
        var notificationId = Guid.NewGuid();
        var command = new MarkAsReadCommand { NotificationId = notificationId };

        _mockInAppRepository.Setup(x => x.MarkAsReadAsync(notificationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockInAppRepository.Verify(x => x.MarkAsReadAsync(notificationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_InAppRepository_Is_Null()
    {
        var act = () => new MarkAsReadCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
