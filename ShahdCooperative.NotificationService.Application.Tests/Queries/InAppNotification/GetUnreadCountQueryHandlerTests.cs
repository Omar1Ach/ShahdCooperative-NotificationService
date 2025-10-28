using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Queries.InAppNotification;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Queries.InAppNotification;

public class GetUnreadCountQueryHandlerTests
{
    private readonly Mock<IInAppNotificationRepository> _mockInAppRepository;
    private readonly GetUnreadCountQueryHandler _handler;

    public GetUnreadCountQueryHandlerTests()
    {
        _mockInAppRepository = new Mock<IInAppNotificationRepository>();
        _handler = new GetUnreadCountQueryHandler(_mockInAppRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Unread_Count()
    {
        var userId = Guid.NewGuid();
        var expectedCount = 5;

        _mockInAppRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        var query = new GetUnreadCountQuery { UserId = userId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(expectedCount);
        _mockInAppRepository.Verify(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Zero_When_No_Unread_Notifications()
    {
        var userId = Guid.NewGuid();

        _mockInAppRepository.Setup(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetUnreadCountQuery { UserId = userId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(0);
        _mockInAppRepository.Verify(x => x.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_InAppRepository_Is_Null()
    {
        var act = () => new GetUnreadCountQueryHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
