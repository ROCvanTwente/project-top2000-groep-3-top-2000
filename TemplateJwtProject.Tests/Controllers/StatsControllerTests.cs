using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TemplateJwtProject.Controllers;
using TemplateJwtProject.Tests.Helpers;
using Xunit;

namespace TemplateJwtProject.Tests.Controllers;

public class StatsControllerTests
{
    [Fact]
    public async Task GetBiggestDrops_WhenDatabaseNotRelational_ReturnsServerError()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetBiggestDrops_WhenDatabaseNotRelational_ReturnsServerError));
        var controller = new StatsController(context);

        var result = await controller.GetBiggestDrops(2024);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(500);
    }
}
