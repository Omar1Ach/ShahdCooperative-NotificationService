using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.API.Controllers;
using ShahdCooperative.NotificationService.Application.DTOs;
using ShahdCooperative.NotificationService.Application.Queries.GetHealthStatus;
using Xunit;

namespace ShahdCooperative.NotificationService.API.Tests.Controllers;

public class HealthControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<HealthController>> _mockLogger;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Mediator_Is_Null()
    {
        var act = () => new HealthController(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new HealthController(_mockMediator.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetHealth_Should_Return_Ok_When_System_Is_Healthy()
    {
        var healthStatus = new HealthStatusDto
        {
            Status = "Healthy",
            IsDatabaseHealthy = true,
            IsQueueHealthy = true,
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetHealthStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthStatus);

        var result = await _controller.GetHealth(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(healthStatus);
    }

    [Fact]
    public async Task GetHealth_Should_Return_503_When_System_Is_Unhealthy()
    {
        var healthStatus = new HealthStatusDto
        {
            Status = "Unhealthy",
            IsDatabaseHealthy = false,
            IsQueueHealthy = false,
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetHealthStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthStatus);

        var result = await _controller.GetHealth(CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(503);
        objectResult.Value.Should().Be(healthStatus);
    }

    [Fact]
    public async Task GetHealth_Should_Return_500_On_Exception()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<GetHealthStatusQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("System error"));

        var result = await _controller.GetHealth(CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
