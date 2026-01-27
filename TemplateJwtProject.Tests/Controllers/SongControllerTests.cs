using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TemplateJwtProject.Controllers;
using TemplateJwtProject.Models;
using TemplateJwtProject.Tests.Helpers;
using Xunit;

namespace TemplateJwtProject.Tests.Controllers;

public class SongControllerTests
{
    [Fact]
    public async Task GetAllSongs_WhenNone_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetAllSongs_WhenNone_ReturnsNotFound));
        var controller = new SongController(context);

        var result = await controller.GetAllSongs();

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSongsWithLyrics_WhenLyricsExist_ReturnsOk()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetSongsWithLyrics_WhenLyricsExist_ReturnsOk));
        var artist = new Artist { ArtistId = 1, Name = "Artist" };
        context.Artists.Add(artist);
        context.Songs.Add(new Song { SongId = 1, Titel = "Song A", ArtistId = artist.ArtistId, Artist = artist, Lyrics = "Words" });
        await context.SaveChangesAsync();
        var controller = new SongController(context);

        var result = await controller.GetSongsWithLyrics();

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeAssignableTo<IEnumerable<object>>();
    }

    [Fact]
    public async Task GetSongLyrics_WhenMissing_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetSongLyrics_WhenMissing_ReturnsNotFound));
        context.Songs.Add(new Song { SongId = 3, Titel = "No Lyrics", ArtistId = 1 });
        await context.SaveChangesAsync();
        var controller = new SongController(context);

        var result = await controller.GetSongLyrics(3);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
