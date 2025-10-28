using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Services.Email;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services.Email;

public class AwsSesEmailSenderTests
{
    private readonly Mock<ILogger<AwsSesEmailSender>> _mockLogger;
    private readonly EmailSettings _validSettings;

    public AwsSesEmailSenderTests()
    {
        _mockLogger = new Mock<ILogger<AwsSesEmailSender>>();
        _validSettings = new EmailSettings
        {
            AwsSesAccessKey = "AKIAIOSFODNN7EXAMPLE",
            AwsSesSecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
            AwsSesRegion = "us-east-1",
            FromEmail = "noreply@example.com",
            FromName = "Test Sender"
        };
    }

    [Fact]
    public void Constructor_Should_Set_NotificationType_To_Email()
    {
        var sender = new AwsSesEmailSender(_mockLogger.Object, Options.Create(_validSettings));

        sender.NotificationType.Should().Be(NotificationType.Email);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new AwsSesEmailSender(null!, Options.Create(_validSettings));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Is_Null()
    {
        var act = () => new AwsSesEmailSender(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_AccessKey_Is_Empty()
    {
        var settings = new EmailSettings
        {
            AwsSesAccessKey = "",
            AwsSesSecretKey = "secret",
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new AwsSesEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test Subject", "Test Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_SecretKey_Is_Empty()
    {
        var settings = new EmailSettings
        {
            AwsSesAccessKey = "accesskey",
            AwsSesSecretKey = "",
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new AwsSesEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test Subject", "Test Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Credentials_Are_Null()
    {
        var settings = new EmailSettings
        {
            AwsSesAccessKey = null!,
            AwsSesSecretKey = null!,
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new AwsSesEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test Subject", "Test Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Log_Error_When_Credentials_Not_Configured()
    {
        var settings = new EmailSettings
        {
            AwsSesAccessKey = "",
            AwsSesSecretKey = ""
        };
        var sender = new AwsSesEmailSender(_mockLogger.Object, Options.Create(settings));

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
    public async Task SendAsync_Should_Return_False_For_Invalid_Credentials()
    {
        var settings = new EmailSettings
        {
            AwsSesAccessKey = "invalid_access_key",
            AwsSesSecretKey = "invalid_secret_key",
            AwsSesRegion = "us-east-1",
            FromEmail = "test@example.com",
            FromName = "Test"
        };

        var sender = new AwsSesEmailSender(_mockLogger.Object, Options.Create(settings));

        var result = await sender.SendAsync("recipient@example.com", "Test", "Body");

        result.Should().BeFalse();
    }
}
