using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShahdCooperative.NotificationService.Infrastructure.Configuration;
using ShahdCooperative.NotificationService.Infrastructure.Services.Push;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services.Push;

public class FirebasePushNotificationSenderTests
{
    private readonly Mock<ILogger<FirebasePushNotificationSender>> _mockLogger;

    public FirebasePushNotificationSenderTests()
    {
        _mockLogger = new Mock<ILogger<FirebasePushNotificationSender>>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var settings = new PushNotificationSettings
        {
            FirebaseCredentialsPath = "test.json",
            FirebaseProjectId = "test-project"
        };

        var act = () => new FirebasePushNotificationSender(null!, Options.Create(settings));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Is_Null()
    {
        var act = () => new FirebasePushNotificationSender(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_CredentialsPath_Is_Empty()
    {
        var invalidSettings = new PushNotificationSettings
        {
            FirebaseCredentialsPath = "",
            FirebaseProjectId = "test-project"
        };

        var act = () => new FirebasePushNotificationSender(_mockLogger.Object, Options.Create(invalidSettings));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Firebase credentials path is not configured");
    }

    [Fact]
    public void Constructor_Should_Throw_When_CredentialsFile_Does_Not_Exist()
    {
        var invalidSettings = new PushNotificationSettings
        {
            FirebaseCredentialsPath = "C:\\NonExistent\\Path\\credentials.json",
            FirebaseProjectId = "test-project"
        };

        var act = () => new FirebasePushNotificationSender(_mockLogger.Object, Options.Create(invalidSettings));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Firebase credentials file not found at: *");
    }

}
