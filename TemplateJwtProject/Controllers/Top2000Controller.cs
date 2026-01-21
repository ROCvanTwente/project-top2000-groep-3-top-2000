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
    /// Gets the top 10 songs from the Top 2000 for a specific year
    /// </summary>
    /// <param name="year">The year to retrieve Top 2000 entries for (default: 2024)</param>
    /// <returns>List of top 10 entries with position, song title, and artist name</returns>
    [HttpGet("top10")]
    public IActionResult GetTop10(int year = 2024)
    {
        var top10 = _context.Top2000Entries
            .Include(t => t.Song)
                .ThenInclude(s => s!.Artist)
            .Where(t => t.Year == year)
            .OrderBy(t => t.Position)
            .Take(10)
            .Select(t => new
            {
                Entry = t,
                PreviousYearPosition = _context.Top2000Entries
                    .Where(prev => prev.SongId == t.SongId && prev.Year == year - 1)
                    .Select(prev => (int?)prev.Position)
                    .FirstOrDefault()
            })
            .ToList()
            .Select(t => new Top2000EntryDto
            {
                Position = t.Entry.Position,
                SongId = t.Entry.Song!.SongId,
                Titel = t.Entry.Song.Titel,
                Artist = t.Entry.Song.Artist!.Name,
                Trend = CalculateTrend(t.Entry.Position, t.PreviousYearPosition)
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
            .Select(t => new
            {
                Entry = t,
                PreviousYearPosition = _context.Top2000Entries
                    .Where(prev => prev.SongId == t.SongId && prev.Year == year - 1)
                    .Select(prev => (int?)prev.Position)
                    .FirstOrDefault()
            })
            .ToList()
            .Select(t => new Top2000EntryDto
            {
                Position = t.Entry.Position,
                SongId = t.Entry.Song!.SongId,
                Titel = t.Entry.Song.Titel,
                Artist = t.Entry.Song.Artist!.Name,
                Trend = CalculateTrend(t.Entry.Position, t.PreviousYearPosition)
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
        var entryData = _context.Top2000Entries
            .Include(t => t.Song)
                .ThenInclude(s => s!.Artist)
            .Where(t => t.Position == position && t.Year == year)
            .Select(t => new
            {
                Entry = t,
                PreviousYearPosition = _context.Top2000Entries
                    .Where(prev => prev.SongId == t.SongId && prev.Year == year - 1)
                    .Select(prev => (int?)prev.Position)
                    .FirstOrDefault()
            })
            .FirstOrDefault();

        if (entryData == null)
        {
            return NotFound(new { message = $"No entry found at position {position} for year {year}" });
        }

        var entry = new Top2000EntryDto
        {
            Position = entryData.Entry.Position,
            SongId = entryData.Entry.Song!.SongId,
            Titel = entryData.Entry.Song!.Titel,
            Artist = entryData.Entry.Song.Artist!.Name,
            Trend = CalculateTrend(entryData.Entry.Position, entryData.PreviousYearPosition)
        };

        return Ok(entry);
    }

    /// <summary>
    /// Calculates the trend based on current and previous year positions
    /// </summary>
    /// <param name="currentPosition">Current year position</param>
    /// <param name="previousPosition">Previous year position (null if not in list)</param>
    /// <returns>Positive if song moved up, negative if moved down, 0 if no change or invalid data</returns>
    private int CalculateTrend(int currentPosition, int? previousPosition)
    {
        try
        {
            if (!previousPosition.HasValue)
            {
                return 0;
            }

            return previousPosition.Value - currentPosition;
        }
        catch
        {
            return 0;
        }
    }
}
