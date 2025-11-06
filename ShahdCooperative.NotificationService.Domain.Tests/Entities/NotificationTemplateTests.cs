using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Tests.Entities;

public class NotificationTemplateTests
{
    [Fact]
    public void NotificationTemplate_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var template = new NotificationTemplate();

        // Assert
        template.Id.Should().Be(Guid.Empty);
        template.Key.Should().BeEmpty();
        template.Name.Should().BeEmpty();
        template.Subject.Should().BeNull();
        template.Body.Should().BeEmpty();
        template.IsActive.Should().BeTrue();
        template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void NotificationTemplate_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var templateKey = "user.registered";
        var templateName = "Welcome Email";
        var subject = "Welcome to ShahdCooperative";
        var bodyTemplate = "<html><body>Welcome {{firstName}}</body></html>";
        var notificationType = NotificationType.Email;

        // Act
        var template = new NotificationTemplate
        {
            Id = id,
            Key = templateKey,
            Name = templateName,
            Subject = subject,
            Body = bodyTemplate,
            Type = notificationType,
            IsActive = false
        };

        // Assert
        template.Id.Should().Be(id);
        template.Key.Should().Be(templateKey);
        template.Name.Should().Be(templateName);
        template.Subject.Should().Be(subject);
        template.Body.Should().Be(bodyTemplate);
        template.Type.Should().Be(notificationType);
        template.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(NotificationType.Email)]
    [InlineData(NotificationType.SMS)]
    [InlineData(NotificationType.Push)]
    [InlineData(NotificationType.InApp)]
    [InlineData(NotificationType.All)]
    public void NotificationTemplate_Should_Support_All_Types(NotificationType type)
    {
        // Arrange & Act
        var template = new NotificationTemplate
        {
            Type = type
        };

        // Assert
        template.Type.Should().Be(type);
    }
}
