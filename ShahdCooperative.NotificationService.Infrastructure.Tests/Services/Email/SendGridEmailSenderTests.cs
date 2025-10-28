using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Services.Email;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services.Email;

public class SendGridEmailSenderTests
{
    private readonly Mock<ILogger<SendGridEmailSender>> _mockLogger;
    private readonly EmailSettings _validSettings;

    public SendGridEmailSenderTests()
    {
        _mockLogger = new Mock<ILogger<SendGridEmailSender>>();
        _validSettings = new EmailSettings
        {
            SendGridApiKey = "SG.test_api_key_12345",
            FromEmail = "noreply@example.com",
            FromName = "Test Sender"
        };
    }

    [Fact]
    public void Constructor_Should_Set_NotificationType_To_Email()
    {
        var sender = new SendGridEmailSender(_mockLogger.Object, Options.Create(_validSettings));

        sender.NotificationType.Should().Be(NotificationType.Email);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new SendGridEmailSender(null!, Options.Create(_validSettings));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Is_Null()
    {
        var act = () => new SendGridEmailSender(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_ApiKey_Is_Empty()
    {
        var settings = new EmailSettings
        {
            SendGridApiKey = "",
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new SendGridEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test Subject", "Test Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_ApiKey_Is_Null()
    {
        var settings = new EmailSettings
        {
            SendGridApiKey = null!,
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new SendGridEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test Subject", "Test Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Log_Error_When_ApiKey_Not_Configured()
    {
        var settings = new EmailSettings { SendGridApiKey = "" };
        var sender = new SendGridEmailSender(_mockLogger.Object, Options.Create(settings));

        await sender.SendAsync("recipient@example.com", "Test", "Body");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_For_Invalid_ApiKey()
    {
        var settings = new EmailSettings
        {
            SendGridApiKey = "invalid_api_key",
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new SendGridEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test", "Body");

        result.Should().BeFalse();
    }
}
