using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TemplateJwtProject.Controllers;
using TemplateJwtProject.Models;
using TemplateJwtProject.Tests.Helpers;
using Xunit;

namespace TemplateJwtProject.Tests.Controllers;

public class ArtistControllerTests
{
    [Fact]
    public async Task GetAllArtists_WhenDatabaseEmpty_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetAllArtists_WhenDatabaseEmpty_ReturnsNotFound));
        var controller = new ArtistController(context);

        var result = await controller.GetAllArtists();

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetArtistById_WhenArtistMissing_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetArtistById_WhenArtistMissing_ReturnsNotFound));
        var controller = new ArtistController(context);

        var result = await controller.GetArtistById(42);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task SearchArtists_WhenMatchExists_ReturnsOkWithArtists()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(SearchArtists_WhenMatchExists_ReturnsOkWithArtists));
        context.Artists.Add(new Artist { ArtistId = 1, Name = "Queen" });
        context.Artists.Add(new Artist { ArtistId = 2, Name = "The Beatles" });
        await context.SaveChangesAsync();
        var controller = new ArtistController(context);

        var result = await controller.SearchArtists("Queen");

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeAssignableTo<IEnumerable<object>>();
    }

    [Fact]
    public async Task GetArtistSongs_WhenNoSongs_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetArtistSongs_WhenNoSongs_ReturnsNotFound));
        context.Artists.Add(new Artist { ArtistId = 5, Name = "NoSongs" });
        await context.SaveChangesAsync();
        var controller = new ArtistController(context);

        var result = await controller.GetArtistSongs(5);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
