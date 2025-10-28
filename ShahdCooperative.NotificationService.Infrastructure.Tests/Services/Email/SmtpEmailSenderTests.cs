using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Services.Email;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services.Email;

public class SmtpEmailSenderTests
{
    private readonly Mock<ILogger<SmtpEmailSender>> _mockLogger;
    private readonly EmailSettings _validSettings;

    public SmtpEmailSenderTests()
    {
        _mockLogger = new Mock<ILogger<SmtpEmailSender>>();
        _validSettings = new EmailSettings
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUsername = "user@example.com",
            SmtpPassword = "password",
            FromEmail = "noreply@example.com",
            FromName = "Test Sender"
        };
    }

    [Fact]
    public void Constructor_Should_Set_NotificationType_To_Email()
    {
        var sender = new SmtpEmailSender(_mockLogger.Object, Options.Create(_validSettings));

        sender.NotificationType.Should().Be(NotificationType.Email);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new SmtpEmailSender(null!, Options.Create(_validSettings));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Is_Null()
    {
        var act = () => new SmtpEmailSender(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_SmtpHost_Is_Empty()
    {
        var settings = new EmailSettings
        {
            SmtpHost = "",
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new SmtpEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test Subject", "Test Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_SmtpHost_Is_Null()
    {
        var settings = new EmailSettings
        {
            SmtpHost = null!,
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new SmtpEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test Subject", "Test Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Log_Error_When_SmtpHost_Not_Configured()
    {
        var settings = new EmailSettings { SmtpHost = "" };
        var sender = new SmtpEmailSender(_mockLogger.Object, Options.Create(settings));

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
    public async Task SendAsync_Should_Return_False_For_Invalid_Smtp_Server()
    {
        var settings = new EmailSettings
        {
            SmtpHost = "invalid.smtp.server.that.does.not.exist",
            SmtpPort = 587,
            SmtpUsername = "user",
            SmtpPassword = "pass",
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new SmtpEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test", "Body");

        result.Should().BeFalse();
    }
}
