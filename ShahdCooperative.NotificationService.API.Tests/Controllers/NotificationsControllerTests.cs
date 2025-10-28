using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.API.Controllers;
using ShahdCooperative.NotificationService.Application.Commands.SendNotification;
using ShahdCooperative.NotificationService.Domain.Enums;
using Xunit;

namespace ShahdCooperative.NotificationService.API.Tests.Controllers;

public class NotificationsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<NotificationsController>> _mockLogger;
    private readonly NotificationsController _controller;

    public NotificationsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<NotificationsController>>();
        _controller = new NotificationsController(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Mediator_Is_Null()
    {
        var act = () => new NotificationsController(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new NotificationsController(_mockMediator.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendNotification_Should_Return_Ok_With_NotificationId()
    {
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.Email,
            Recipient = "user@example.com",
            Subject = "Test",
            Body = "Test message"
        };

        var notificationId = Guid.NewGuid();
        _mockMediator.Setup(x => x.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notificationId);

        var result = await _controller.SendNotification(command, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task SendNotification_Should_Return_500_On_Exception()
    {
        var command = new SendNotificationCommand
        {
            NotificationType = NotificationType.Email,
            Recipient = "user@example.com",
            Subject = "Test",
            Body = "Test message"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.SendNotification(command, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
