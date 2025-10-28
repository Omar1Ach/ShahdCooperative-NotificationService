using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Queries.InAppNotification;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;
using InAppNotificationEntity = ShahdCooperative.NotificationService.Domain.Entities.InAppNotification;

namespace ShahdCooperative.NotificationService.Application.Tests.Queries.InAppNotification;

public class GetUserNotificationsQueryHandlerTests
{
    private readonly Mock<IInAppNotificationRepository> _mockInAppRepository;
    private readonly GetUserNotificationsQueryHandler _handler;

    public GetUserNotificationsQueryHandlerTests()
    {
        _mockInAppRepository = new Mock<IInAppNotificationRepository>();
        _handler = new GetUserNotificationsQueryHandler(_mockInAppRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_User_Notifications()
    {
        var userId = Guid.NewGuid();
        var expectedNotifications = new List<InAppNotificationEntity>
        {
            new InAppNotificationEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Notification 1",
                Message = "Message 1",
                Type = InAppNotificationType.Info,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new InAppNotificationEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Notification 2",
                Message = "Message 2",
                Type = InAppNotificationType.Warning,
                IsRead = true,
                ReadAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockInAppRepository.Setup(x => x.GetUserNotificationsAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNotifications);

        var query = new GetUserNotificationsQuery { UserId = userId, PageNumber = 1, PageSize = 20 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedNotifications);
        _mockInAppRepository.Verify(x => x.GetUserNotificationsAsync(userId, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Notifications()
    {
        var userId = Guid.NewGuid();

        _mockInAppRepository.Setup(x => x.GetUserNotificationsAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InAppNotificationEntity>());

        var query = new GetUserNotificationsQuery { UserId = userId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockInAppRepository.Verify(x => x.GetUserNotificationsAsync(userId, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_InAppRepository_Is_Null()
    {
        var act = () => new GetUserNotificationsQueryHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
