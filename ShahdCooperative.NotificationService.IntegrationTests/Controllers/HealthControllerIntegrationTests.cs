using System.Net;
using FluentAssertions;

namespace ShahdCooperative.NotificationService.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class HealthControllerIntegrationTests : IntegrationTestBase 
{

    public HealthControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }



    [Fact]
    public async Task Health_ReturnsOk()
    {
        // Act
        var response = await Client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
}
