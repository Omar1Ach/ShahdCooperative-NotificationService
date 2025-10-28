using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Queries.Template;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Queries.Template;

public class GetTemplateByKeyQueryHandlerTests
{
    private readonly Mock<INotificationTemplateRepository> _mockTemplateRepository;
    private readonly GetTemplateByKeyQueryHandler _handler;

    public GetTemplateByKeyQueryHandlerTests()
    {
        _mockTemplateRepository = new Mock<INotificationTemplateRepository>();
        _handler = new GetTemplateByKeyQueryHandler(_mockTemplateRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Template_When_Found()
    {
        var expectedTemplate = new NotificationTemplate
        {
            TemplateKey = "test-template",
            NotificationType = NotificationType.Email,
            TemplateName = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            IsActive = true
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTemplate);

        var query = new GetTemplateByKeyQuery { TemplateKey = "test-template" };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTemplate);
        _mockTemplateRepository.Verify(x => x.GetByKeyAsync(query.TemplateKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Template_Not_Found()
    {
        _mockTemplateRepository.Setup(x => x.GetByKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate?)null);

        var query = new GetTemplateByKeyQuery { TemplateKey = "non-existent" };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
        _mockTemplateRepository.Verify(x => x.GetByKeyAsync(query.TemplateKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_TemplateRepository_Is_Null()
    {
        var act = () => new GetTemplateByKeyQueryHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
