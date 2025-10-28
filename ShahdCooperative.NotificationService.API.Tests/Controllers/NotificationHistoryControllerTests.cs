using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.API.Controllers;
using ShahdCooperative.NotificationService.Application.Queries.NotificationHistory;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using Xunit;

namespace ShahdCooperative.NotificationService.API.Tests.Controllers;

public class NotificationHistoryControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<NotificationHistoryController>> _mockLogger;
    private readonly NotificationHistoryController _controller;

    public NotificationHistoryControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<NotificationHistoryController>>();
        _controller = new NotificationHistoryController(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Mediator_Is_Null()
    {
        var act = () => new NotificationHistoryController(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new NotificationHistoryController(_mockMediator.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetNotificationHistory_Should_Return_Ok_With_History()
    {
        var recipient = "user@example.com";
        var history = new List<NotificationLog>
        {
            new NotificationLog
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.Email,
                Recipient = recipient,
                Subject = "Test",
                Message = "Test message",
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetNotificationHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await _controller.GetNotificationHistory(recipient, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(history);
    }

    [Fact]
    public async Task GetNotificationHistory_Should_Return_Ok_With_Empty_List_When_No_History()
    {
        var recipient = "user@example.com";

        _mockMediator.Setup(x => x.Send(It.IsAny<GetNotificationHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationLog>());

        var result = await _controller.GetNotificationHistory(recipient, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var logs = okResult!.Value as IEnumerable<NotificationLog>;
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNotificationHistory_Should_Return_500_On_Exception()
    {
        var recipient = "user@example.com";

        _mockMediator.Setup(x => x.Send(It.IsAny<GetNotificationHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetNotificationHistory(recipient, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
