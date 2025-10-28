using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Services.Sms;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services.Sms;

public class VonageSmsSenderTests
{
    private readonly Mock<ILogger<VonageSmsSender>> _mockLogger;
    private readonly SmsSettings _settings;

    public VonageSmsSenderTests()
    {
        _mockLogger = new Mock<ILogger<VonageSmsSender>>();
        _settings = new SmsSettings
        {
            Provider = "Vonage",
            VonageApiKey = "test_api_key",
            VonageApiSecret = "test_api_secret",
            VonageFromNumber = "ShahdCoop"
        };
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new VonageSmsSender(null!, Options.Create(_settings));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Is_Null()
    {
        var act = () => new VonageSmsSender(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_ApiKey_Is_Empty()
    {
        var invalidSettings = new SmsSettings
        {
            VonageApiKey = "",
            VonageApiSecret = "test_secret",
            VonageFromNumber = "ShahdCoop"
        };

        var act = () => new VonageSmsSender(_mockLogger.Object, Options.Create(invalidSettings));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Vonage credentials are not configured");
    }

    [Fact]
    public void Constructor_Should_Throw_When_ApiSecret_Is_Empty()
    {
        var invalidSettings = new SmsSettings
        {
            VonageApiKey = "test_key",
            VonageApiSecret = "",
            VonageFromNumber = "ShahdCoop"
        };

        var act = () => new VonageSmsSender(_mockLogger.Object, Options.Create(invalidSettings));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Vonage credentials are not configured");
    }

    [Fact]
    public void NotificationType_Should_Return_SMS()
    {
        var sender = new VonageSmsSender(_mockLogger.Object, Options.Create(_settings));

        sender.NotificationType.Should().Be(NotificationType.SMS);
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Recipient_Is_Empty()
    {
        var sender = new VonageSmsSender(_mockLogger.Object, Options.Create(_settings));

        var result = await sender.SendAsync("", "Subject", "Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Body_Is_Empty()
    {
        var sender = new VonageSmsSender(_mockLogger.Object, Options.Create(_settings));

        var result = await sender.SendAsync("+1234567890", "Subject", "");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Recipient_Is_Whitespace()
    {
        var sender = new VonageSmsSender(_mockLogger.Object, Options.Create(_settings));

        var result = await sender.SendAsync("   ", "Subject", "Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Body_Is_Whitespace()
    {
        var sender = new VonageSmsSender(_mockLogger.Object, Options.Create(_settings));

        var result = await sender.SendAsync("+1234567890", "Subject", "   ");

        result.Should().BeFalse();
    }
}
