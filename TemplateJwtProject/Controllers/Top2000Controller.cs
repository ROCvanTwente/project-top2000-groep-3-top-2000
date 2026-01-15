using Microsoft.AspNetCore.Mvc;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models.DTOs;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Top2000Controller : ControllerBase
{
    private readonly AppDbContext _context;

    public Top2000Controller(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the top 10 songs from the Top 2000 for a specific year
    /// </summary>
    /// <param name="year">The year to retrieve Top 2000 entries for (default: 2024)</param>
    /// <returns>List of top 10 entries with position, song title, and artist name</returns>
    [HttpGet("top10")]
    public IActionResult GetTop10(int year = 2024)
    {
        var top10 = _context.Top2000Entries
            .Where(t => t.Year == year)
            .OrderBy(t => t.Position)
            .Take(10)
            .Select(t => new Top2000EntryDto
            {
                Position = t.Position,
                SongId = t.Song!.SongId,
                Titel = t.Song.Titel,
                Artist = t.Song.Artist!.Name
            })
            .ToList();

        if (!top10.Any())
        {
            return NotFound(new { message = $"No Top 2000 entries found for year {year}" });
        }

        return Ok(top10);
    }

    /// <summary>
    /// Gets all entries from the Top 2000 for a specific year
    /// </summary>
    /// <param name="year">The year to retrieve Top 2000 entries for (default: 2024)</param>
    /// <returns>List of all entries for that year, sorted by position</returns>
    [HttpGet("by-year/{year}")]
    public IActionResult GetByYear(int year)
    {
        var entries = _context.Top2000Entries
            .Where(t => t.Year == year)
            .OrderBy(t => t.Position)
            .Select(t => new Top2000EntryDto
            {
                Position = t.Position,
                SongId = t.Song!.SongId,
                Titel = t.Song.Titel,
                Artist = t.Song.Artist!.Name
            })
            .ToList();

        if (!entries.Any())
        {
            return NotFound(new { message = $"No Top 2000 entries found for year {year}" });
        }

        return Ok(entries);
    }

    /// <summary>
    /// Gets a specific entry by position and year
    /// </summary>
    /// <param name="position">The position in the Top 2000</param>
    /// <param name="year">The year (default: 2024)</param>
    /// <returns>Single entry details</returns>
    [HttpGet("{position}")]
    public IActionResult GetByPosition(int position, int year = 2024)
    {
        var entry = _context.Top2000Entries
            .Where(t => t.Position == position && t.Year == year)
            .Select(t => new Top2000EntryDto
            {
                Position = t.Position,
                SongId = t.Song!.SongId,
                Titel = t.Song!.Titel,
                Artist = t.Song.Artist!.Name
            })
            .FirstOrDefault();

        if (entry == null)
        {
            return NotFound(new { message = $"No entry found at position {position} for year {year}" });
        }

        return Ok(entry);
    }
}
