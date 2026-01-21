using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    /// Calculates the trend (position change) from the previous year
    /// </summary>
    private int CalculateTrend(int songId, int currentYear)
    {
        try
        {
            var currentEntry = _context.Top2000Entries
                .FirstOrDefault(t => t.SongId == songId && t.Year == currentYear);

            if (currentEntry == null)
                return 0;

            var previousYearEntry = _context.Top2000Entries
                .FirstOrDefault(t => t.SongId == songId && t.Year == currentYear - 1);

            if (previousYearEntry == null)
                return 0;

            // Trend: negative = went down, positive = went up
            return previousYearEntry.Position - currentEntry.Position;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the top 10 songs from the Top 2000 for a specific year
    /// </summary>
    /// <param name="year">The year to retrieve Top 2000 entries for (default: 2024)</param>
    /// <returns>List of top 10 entries with position, song title, artist name, and trend</returns>
    [HttpGet("top10")]
    public IActionResult GetTop10(int year = 2024)
    {
        var top10 = _context.Top2000Entries
            .Include(t => t.Song)
                .ThenInclude(s => s!.Artist)
            .Where(t => t.Year == year)
            .OrderBy(t => t.Position)
            .Take(10)
            .AsEnumerable()
            .Select(t => new Top2000EntryDto
            {
                Position = t.Position,
                Year = t.Year,
                SongId = t.SongId,
                Titel = t.Song!.Titel,
                Artist = t.Song.Artist!.Name,
                Trend = CalculateTrend(t.SongId, year)
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
            .Include(t => t.Song)
                .ThenInclude(s => s!.Artist)
            .Where(t => t.Year == year)
            .OrderBy(t => t.Position)
            .AsEnumerable()
            .Select(t => new Top2000EntryDto
            {
                Position = t.Position,
                Year = t.Year,
                SongId = t.SongId,
                Titel = t.Song!.Titel,
                Artist = t.Song.Artist!.Name,
                Trend = CalculateTrend(t.SongId, year)
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
    /// <returns>Single entry details with trend</returns>
    [HttpGet("{position}")]
    public IActionResult GetByPosition(int position, int year = 2024)
    {
        var entry = _context.Top2000Entries
            .Include(t => t.Song)
                .ThenInclude(s => s!.Artist)
            .Where(t => t.Position == position && t.Year == year)
            .AsEnumerable()
            .Select(t => new Top2000EntryDto
            {
                Position = t.Position,
                Year = t.Year,
                SongId = t.SongId,
                Titel = t.Song!.Titel,
                Artist = t.Song.Artist!.Name,
                Trend = CalculateTrend(t.SongId, year)
            })
            .FirstOrDefault();

        if (entry == null)
        {
            return NotFound(new { message = $"No entry found at position {position} for year {year}" });
        }

        return Ok(entry);
    }
}
