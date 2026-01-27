using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TemplateJwtProject.Controllers;
using TemplateJwtProject.Models;
using TemplateJwtProject.Tests.Helpers;
using Xunit;

namespace TemplateJwtProject.Tests.Controllers;

public class Top2000ControllerTests
{
    [Fact]
    public void GetTop10_WhenEntriesExist_ReturnsOkWithTrend()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetTop10_WhenEntriesExist_ReturnsOkWithTrend));
        var artist = new Artist { ArtistId = 1, Name = "Artist" };
        var song = new Song { SongId = 1, Titel = "Song", ArtistId = artist.ArtistId, Artist = artist };

        context.Artists.Add(artist);
        context.Songs.Add(song);
        context.Top2000Entries.AddRange(
            new Top2000Entry { SongId = song.SongId, Song = song, Year = 2024, Position = 2 },
            new Top2000Entry { SongId = song.SongId, Song = song, Year = 2023, Position = 5 }
        );
        context.SaveChanges();

        var controller = new Top2000Controller(context);

        var result = controller.GetTop10(2024);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var payload = okResult!.Value as IEnumerable<object>;
        payload.Should().NotBeNull();
    }

    [Fact]
    public void GetByPosition_WhenMissing_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.CreateContext(nameof(GetByPosition_WhenMissing_ReturnsNotFound));
        var controller = new Top2000Controller(context);

        var result = controller.GetByPosition(999, 2024);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
