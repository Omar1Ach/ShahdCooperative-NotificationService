using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Queries.Template;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Queries.Template;

public class GetAllTemplatesQueryHandlerTests
{
    private readonly Mock<INotificationTemplateRepository> _mockTemplateRepository;
    private readonly GetAllTemplatesQueryHandler _handler;

    public GetAllTemplatesQueryHandlerTests()
    {
        _mockTemplateRepository = new Mock<INotificationTemplateRepository>();
        _handler = new GetAllTemplatesQueryHandler(_mockTemplateRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Templates()
    {
        var expectedTemplates = new List<NotificationTemplate>
        {
            new NotificationTemplate
            {
                Key = "template-1",
                Type = NotificationType.Email,
                Name = "Template 1",
                Subject = "Subject 1",
                Body = "Body 1",
                IsActive = true
            },
            new NotificationTemplate
            {
                Key = "template-2",
                Type = NotificationType.SMS,
                Name = "Template 2",
                Subject = string.Empty,
                Body = "Body 2",
                IsActive = false
            }
        };

        _mockTemplateRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTemplates);

        var query = new GetAllTemplatesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedTemplates);
        _mockTemplateRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Templates()
    {
        _mockTemplateRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationTemplate>());

        var query = new GetAllTemplatesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockTemplateRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_TemplateRepository_Is_Null()
    {
        var act = () => new GetAllTemplatesQueryHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
