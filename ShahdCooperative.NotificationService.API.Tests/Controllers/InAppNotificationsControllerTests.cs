using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.API.Controllers;
using ShahdCooperative.NotificationService.Application.Commands.InAppNotification;
using ShahdCooperative.NotificationService.Application.Queries.InAppNotification;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using Xunit;
using InAppNotificationEntity = ShahdCooperative.NotificationService.Domain.Entities.InAppNotification;

namespace ShahdCooperative.NotificationService.API.Tests.Controllers;

public class InAppNotificationsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<InAppNotificationsController>> _mockLogger;
    private readonly InAppNotificationsController _controller;

    public InAppNotificationsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<InAppNotificationsController>>();
        _controller = new InAppNotificationsController(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Mediator_Is_Null()
    {
        var act = () => new InAppNotificationsController(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new InAppNotificationsController(_mockMediator.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetUserNotifications_Should_Return_Ok_With_Notifications()
    {
        var userId = Guid.NewGuid();
        var notifications = new List<InAppNotificationEntity>
        {
            new InAppNotificationEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test",
                Message = "Test message",
                Type = InAppNotificationType.Info,
                IsRead = false
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var result = await _controller.GetUserNotifications(userId, 1, 20, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(notifications);
    }

    [Fact]
    public async Task GetUserNotifications_Should_Return_Ok_With_Empty_List()
    {
        var userId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InAppNotificationEntity>());

        var result = await _controller.GetUserNotifications(userId, 1, 20, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var notifs = okResult!.Value as IEnumerable<InAppNotificationEntity>;
        notifs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserNotifications_Should_Return_500_On_Exception()
    {
        var userId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetUserNotifications(userId, 1, 20, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetUnreadCount_Should_Return_Ok_With_Count()
    {
        var userId = Guid.NewGuid();
        var count = 5;

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUnreadCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        var result = await _controller.GetUnreadCount(userId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUnreadCount_Should_Return_500_On_Exception()
    {
        var userId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUnreadCountQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetUnreadCount(userId, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task MarkAsRead_Should_Return_NoContent_When_Successful()
    {
        var notificationId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<MarkAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.MarkAsRead(notificationId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task MarkAsRead_Should_Return_NotFound_When_Notification_Not_Found()
    {
        var notificationId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<MarkAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.MarkAsRead(notificationId, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task MarkAsRead_Should_Return_500_On_Exception()
    {
        var notificationId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<MarkAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.MarkAsRead(notificationId, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task MarkAllAsRead_Should_Return_NoContent_When_Successful()
    {
        var userId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<MarkAllAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.MarkAllAsRead(userId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task MarkAllAsRead_Should_Return_NotFound_When_User_Not_Found()
    {
        var userId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<MarkAllAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.MarkAllAsRead(userId, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task MarkAllAsRead_Should_Return_500_On_Exception()
    {
        var userId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<MarkAllAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.MarkAllAsRead(userId, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
