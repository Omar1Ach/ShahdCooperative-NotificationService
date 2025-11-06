using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.API.Controllers;
using ShahdCooperative.NotificationService.Application.Commands.UserPreference;
using ShahdCooperative.NotificationService.Application.Queries.UserPreference;
using ShahdCooperative.NotificationService.Domain.Entities;
using Xunit;

namespace ShahdCooperative.NotificationService.API.Tests.Controllers;

public class UserPreferencesControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<UserPreferencesController>> _mockLogger;
    private readonly UserPreferencesController _controller;

    public UserPreferencesControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<UserPreferencesController>>();
        _controller = new UserPreferencesController(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Mediator_Is_Null()
    {
        var act = () => new UserPreferencesController(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new UserPreferencesController(_mockMediator.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetUserPreferences_Should_Return_Ok_When_Preferences_Found()
    {
        var userId = Guid.NewGuid();
        var preferences = new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = false,
            PushEnabled = true,
            InAppEnabled = true
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserPreferenceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(preferences);

        var result = await _controller.GetUserPreferences(userId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(preferences);
    }

    [Fact]
    public async Task GetUserPreferences_Should_Return_NotFound_When_Preferences_Not_Found()
    {
        var userId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserPreferenceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationPreference?)null);

        var result = await _controller.GetUserPreferences(userId, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserPreferences_Should_Return_500_On_Exception()
    {
        var userId = Guid.NewGuid();

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserPreferenceQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetUserPreferences(userId, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CreateUserPreferences_Should_Return_CreatedAtAction()
    {
        var command = new CreateUserPreferenceCommand
        {
            UserId = Guid.NewGuid(),
            EmailEnabled = true,
            SmsEnabled = false
        };

        var preferenceId = Guid.NewGuid();
        _mockMediator.Setup(x => x.Send(It.IsAny<CreateUserPreferenceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(preferenceId);

        var result = await _controller.CreateUserPreferences(command, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(UserPreferencesController.GetUserPreferences));
    }

    [Fact]
    public async Task CreateUserPreferences_Should_Return_500_On_Exception()
    {
        var command = new CreateUserPreferenceCommand
        {
            UserId = Guid.NewGuid()
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<CreateUserPreferenceCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.CreateUserPreferences(command, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateUserPreferences_Should_Return_NoContent_When_Successful()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailEnabled = false,
            SmsEnabled = true
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateUserPreferenceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.UpdateUserPreferences(userId, command, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateUserPreferences_Should_Return_BadRequest_When_UserIds_Dont_Match()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserPreferenceCommand
        {
            UserId = Guid.NewGuid(),
            EmailEnabled = false
        };

        var result = await _controller.UpdateUserPreferences(userId, command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateUserPreferences_Should_Return_NotFound_When_Preferences_Not_Found()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailEnabled = false
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateUserPreferenceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.UpdateUserPreferences(userId, command, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateUserPreferences_Should_Return_500_On_Exception()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailEnabled = false
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateUserPreferenceCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.UpdateUserPreferences(userId, command, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
