using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Services.Sms;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services.Sms;

public class TwilioSmsSenderTests
{
    private readonly Mock<ILogger<TwilioSmsSender>> _mockLogger;
    private readonly SmsSettings _settings;

    public TwilioSmsSenderTests()
    {
        _mockLogger = new Mock<ILogger<TwilioSmsSender>>();
        _settings = new SmsSettings
        {
            Provider = "Twilio",
            TwilioAccountSid = "AC_TEST_ACCOUNT_SID",
            TwilioAuthToken = "test_auth_token",
            TwilioPhoneNumber = "+1234567890"
        };
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new TwilioSmsSender(null!, Options.Create(_settings));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Is_Null()
    {
        var act = () => new TwilioSmsSender(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Not_Throw_When_AccountSid_Is_Empty()
    {
        var invalidSettings = new SmsSettings
        {
            TwilioAccountSid = "",
            TwilioAuthToken = "test_token",
            TwilioPhoneNumber = "+1234567890"
        };

        var act = () => new TwilioSmsSender(_mockLogger.Object, Options.Create(invalidSettings));

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_Should_Not_Throw_When_AuthToken_Is_Empty()
    {
        var invalidSettings = new SmsSettings
        {
            TwilioAccountSid = "AC_TEST",
            TwilioAuthToken = "",
            TwilioPhoneNumber = "+1234567890"
        };

        var act = () => new TwilioSmsSender(_mockLogger.Object, Options.Create(invalidSettings));

        act.Should().NotThrow();
    }

    [Fact]
    public void NotificationType_Should_Return_SMS()
    {
        var sender = new TwilioSmsSender(_mockLogger.Object, Options.Create(_settings));

        sender.NotificationType.Should().Be(NotificationType.SMS);
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Recipient_Is_Empty()
    {
        var sender = new TwilioSmsSender(_mockLogger.Object, Options.Create(_settings));

        var result = await sender.SendAsync("", "Subject", "Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Body_Is_Empty()
    {
        var sender = new TwilioSmsSender(_mockLogger.Object, Options.Create(_settings));

        var result = await sender.SendAsync("+1234567890", "Subject", "");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Recipient_Is_Whitespace()
    {
        var sender = new TwilioSmsSender(_mockLogger.Object, Options.Create(_settings));

        var result = await sender.SendAsync("   ", "Subject", "Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Body_Is_Whitespace()
    {
        var sender = new TwilioSmsSender(_mockLogger.Object, Options.Create(_settings));

        var result = await sender.SendAsync("+1234567890", "Subject", "   ");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Credentials_Not_Configured()
    {
        var invalidSettings = new SmsSettings
        {
            TwilioAccountSid = "",
            TwilioAuthToken = "",
            TwilioPhoneNumber = "+1234567890"
        };

        var sender = new TwilioSmsSender(_mockLogger.Object, Options.Create(invalidSettings));

        var result = await sender.SendAsync("+1234567890", "Subject", "Test message");

        result.Should().BeFalse();
    }
}
