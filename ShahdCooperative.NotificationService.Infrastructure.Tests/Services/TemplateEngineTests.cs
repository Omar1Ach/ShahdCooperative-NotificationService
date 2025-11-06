using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Services;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services;

public class TemplateEngineTests
{
    private readonly Mock<ILogger<TemplateEngine>> _mockLogger;
    private readonly Mock<INotificationTemplateRepository> _mockTemplateRepository;
    private readonly TemplateEngine _templateEngine;

    public TemplateEngineTests()
    {
        _mockLogger = new Mock<ILogger<TemplateEngine>>();
        _mockTemplateRepository = new Mock<INotificationTemplateRepository>();
        _templateEngine = new TemplateEngine(_mockLogger.Object, _mockTemplateRepository.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new TemplateEngine(null!, _mockTemplateRepository.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_TemplateRepository_Is_Null()
    {
        var act = () => new TemplateEngine(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessTemplateAsync_Should_Return_Processed_Body_With_Valid_Template_And_Data()
    {
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Key = "welcome-email",
            Type = NotificationType.Email,
            Subject = "Welcome {{UserName}}",
            Body = "Hello {{UserName}}, welcome to {{CompanyName}}!",
            IsActive = true
        };

        var templateData = "{\"UserName\":\"John Doe\",\"CompanyName\":\"Shahd Cooperative\"}";

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync("welcome-email", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _templateEngine.ProcessTemplateAsync("welcome-email", templateData);

        result.Should().Be("Hello John Doe, welcome to Shahd Cooperative!");
    }

    [Fact]
    public async Task ProcessTemplateAsync_Should_Return_Empty_When_Template_Not_Found()
    {
        _mockTemplateRepository.Setup(x => x.GetByKeyAsync("non-existent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate?)null);

        var result = await _templateEngine.ProcessTemplateAsync("non-existent", "{}");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessTemplateAsync_Should_Return_Empty_When_Template_Is_Inactive()
    {
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Key = "inactive-template",
            Type = NotificationType.Email,
            Subject = "Test",
            Body = "Test Body",
            IsActive = false
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync("inactive-template", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _templateEngine.ProcessTemplateAsync("inactive-template", "{}");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessTemplateAsync_Should_Handle_Invalid_Json_Data()
    {
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Key = "test-template",
            Type = NotificationType.Email,
            Subject = "Test",
            Body = "Hello {{UserName}}",
            IsActive = true
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync("test-template", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _templateEngine.ProcessTemplateAsync("test-template", "invalid json");

        // Should remove unreplaced placeholders
        result.Should().Be("Hello ");
    }

    [Fact]
    public async Task ProcessTemplateAsync_Should_Return_Empty_When_Exception_Occurs()
    {
        _mockTemplateRepository.Setup(x => x.GetByKeyAsync("error-template", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _templateEngine.ProcessTemplateAsync("error-template", "{}");

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTokens_Should_Replace_All_Tokens()
    {
        var template = "Hello {{Name}}, your order {{OrderId}} is ready!";
        var tokens = new Dictionary<string, string>
        {
            { "Name", "John" },
            { "OrderId", "12345" }
        };

        var result = _templateEngine.ReplaceTokens(template, tokens);

        result.Should().Be("Hello John, your order 12345 is ready!");
    }

    [Fact]
    public void ReplaceTokens_Should_Return_Empty_For_Empty_Template()
    {
        var tokens = new Dictionary<string, string>
        {
            { "Name", "John" }
        };

        var result = _templateEngine.ReplaceTokens(string.Empty, tokens);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTokens_Should_Return_Empty_For_Null_Template()
    {
        var tokens = new Dictionary<string, string>
        {
            { "Name", "John" }
        };

        var result = _templateEngine.ReplaceTokens(null!, tokens);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTokens_Should_Remove_Unreplaced_Placeholders()
    {
        var template = "Hello {{Name}}, your {{UnknownField}} is ready!";
        var tokens = new Dictionary<string, string>
        {
            { "Name", "John" }
        };

        var result = _templateEngine.ReplaceTokens(template, tokens);

        result.Should().Be("Hello John, your  is ready!");
    }

    [Fact]
    public void ReplaceTokens_Should_Handle_Empty_Token_Value()
    {
        var template = "Hello {{Name}}, welcome!";
        var tokens = new Dictionary<string, string>
        {
            { "Name", string.Empty }
        };

        var result = _templateEngine.ReplaceTokens(template, tokens);

        result.Should().Be("Hello , welcome!");
    }

    [Fact]
    public void ReplaceTokens_Should_Handle_Null_Token_Value()
    {
        var template = "Hello {{Name}}, welcome!";
        var tokens = new Dictionary<string, string>
        {
            { "Name", null! }
        };

        var result = _templateEngine.ReplaceTokens(template, tokens);

        result.Should().Be("Hello , welcome!");
    }

    [Fact]
    public void ReplaceTokens_Should_Replace_Tokens_With_Exact_Case_Match()
    {
        var template = "Hello {{name}}, your order is ready!";
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "name", "John" }
        };

        var result = _templateEngine.ReplaceTokens(template, tokens);

        result.Should().Be("Hello John, your order is ready!");
    }

    [Fact]
    public async Task ProcessTemplateAsync_Should_Handle_Empty_Template_Data()
    {
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Key = "simple-template",
            Type = NotificationType.Email,
            Subject = "Test",
            Body = "Hello {{Name}}",
            IsActive = true
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync("simple-template", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _templateEngine.ProcessTemplateAsync("simple-template", string.Empty);

        // Should remove unreplaced placeholders
        result.Should().Be("Hello ");
    }

    [Fact]
    public async Task ProcessTemplateAsync_Should_Handle_Template_Without_Placeholders()
    {
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Key = "static-template",
            Type = NotificationType.Email,
            Subject = "Test",
            Body = "This is a static message",
            IsActive = true
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync("static-template", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _templateEngine.ProcessTemplateAsync("static-template", "{}");

        result.Should().Be("This is a static message");
    }
}
