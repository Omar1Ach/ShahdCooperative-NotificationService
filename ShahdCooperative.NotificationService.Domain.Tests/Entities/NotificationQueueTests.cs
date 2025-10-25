using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Tests.Entities;

public class NotificationQueueTests
{
    [Fact]
    public void NotificationQueue_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var queue = new NotificationQueue();

        // Assert
        queue.Id.Should().Be(Guid.Empty);
        queue.Recipient.Should().BeEmpty();
        queue.Body.Should().BeEmpty();
        queue.Priority.Should().Be(NotificationPriority.Normal);
        queue.Status.Should().Be(NotificationStatus.Pending);
        queue.AttemptCount.Should().Be(0);
        queue.MaxRetries.Should().Be(3);
        queue.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void NotificationQueue_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var body = "Test Body";
        var templateKey = "user.registered";
        var templateData = "{\"firstName\":\"John\"}";

        // Act
        var queue = new NotificationQueue
        {
            Id = id,
            NotificationType = NotificationType.Email,
            Recipient = recipient,
            Subject = subject,
            Body = body,
            TemplateKey = templateKey,
            TemplateData = templateData,
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Processing
        };

        // Assert
        queue.Id.Should().Be(id);
        queue.NotificationType.Should().Be(NotificationType.Email);
        queue.Recipient.Should().Be(recipient);
        queue.Subject.Should().Be(subject);
        queue.Body.Should().Be(body);
        queue.TemplateKey.Should().Be(templateKey);
        queue.TemplateData.Should().Be(templateData);
        queue.Priority.Should().Be(NotificationPriority.High);
        queue.Status.Should().Be(NotificationStatus.Processing);
    }

    [Theory]
    [InlineData(NotificationPriority.Highest, 1)]
    [InlineData(NotificationPriority.High, 2)]
    [InlineData(NotificationPriority.Normal, 5)]
    [InlineData(NotificationPriority.Low, 8)]
    [InlineData(NotificationPriority.Lowest, 10)]
    public void NotificationQueue_Should_Support_Priority_Values(NotificationPriority priority, int expectedValue)
    {
        // Arrange & Act
        var queue = new NotificationQueue
        {
            Priority = priority
        };

        // Assert
        queue.Priority.Should().Be(priority);
        ((int)queue.Priority).Should().Be(expectedValue);
    }

    [Fact]
    public void NotificationQueue_Should_Track_Retry_Attempts()
    {
        // Arrange
        var queue = new NotificationQueue
        {
            AttemptCount = 0,
            MaxRetries = 3
        };

        // Act
        queue.AttemptCount++;
        queue.NextRetryAt = DateTime.UtcNow.AddMinutes(2);
        queue.ErrorMessage = "Connection timeout";

        // Assert
        queue.AttemptCount.Should().Be(1);
        queue.NextRetryAt.Should().NotBeNull();
        queue.ErrorMessage.Should().Be("Connection timeout");
    }

    [Theory]
    [InlineData(NotificationStatus.Pending)]
    [InlineData(NotificationStatus.Processing)]
    [InlineData(NotificationStatus.Sent)]
    [InlineData(NotificationStatus.Failed)]
    [InlineData(NotificationStatus.Cancelled)]
    public void NotificationQueue_Should_Support_All_Statuses(NotificationStatus status)
    {
        // Arrange & Act
        var queue = new NotificationQueue
        {
            Status = status
        };

        // Assert
        queue.Status.Should().Be(status);
    }
}
