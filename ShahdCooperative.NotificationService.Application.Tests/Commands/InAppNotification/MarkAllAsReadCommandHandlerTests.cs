using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.InAppNotification;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.InAppNotification;

public class MarkAllAsReadCommandHandlerTests
{
    private readonly Mock<IInAppNotificationRepository> _mockInAppRepository;
    private readonly MarkAllAsReadCommandHandler _handler;

    public MarkAllAsReadCommandHandlerTests()
    {
        _mockInAppRepository = new Mock<IInAppNotificationRepository>();
        _handler = new MarkAllAsReadCommandHandler(_mockInAppRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Mark_All_Notifications_As_Read_And_Return_True()
    {
        var userId = Guid.NewGuid();
        var command = new MarkAllAsReadCommand { UserId = userId };

        _mockInAppRepository.Setup(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockInAppRepository.Verify(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_InAppRepository_Is_Null()
    {
        var act = () => new MarkAllAsReadCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
