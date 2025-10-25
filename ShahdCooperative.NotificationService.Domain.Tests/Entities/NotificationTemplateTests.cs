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
        template.TemplateKey.Should().BeEmpty();
        template.TemplateName.Should().BeEmpty();
        template.Subject.Should().BeEmpty();
        template.BodyTemplate.Should().BeEmpty();
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
            TemplateKey = templateKey,
            TemplateName = templateName,
            Subject = subject,
            BodyTemplate = bodyTemplate,
            NotificationType = notificationType,
            IsActive = false
        };

        // Assert
        template.Id.Should().Be(id);
        template.TemplateKey.Should().Be(templateKey);
        template.TemplateName.Should().Be(templateName);
        template.Subject.Should().Be(subject);
        template.BodyTemplate.Should().Be(bodyTemplate);
        template.NotificationType.Should().Be(notificationType);
        template.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(NotificationType.Email)]
    [InlineData(NotificationType.SMS)]
    [InlineData(NotificationType.Push)]
    [InlineData(NotificationType.InApp)]
    [InlineData(NotificationType.All)]
    public void NotificationTemplate_Should_Support_All_NotificationTypes(NotificationType type)
    {
        // Arrange & Act
        var template = new NotificationTemplate
        {
            NotificationType = type
        };

        // Assert
        template.NotificationType.Should().Be(type);
    }
}
