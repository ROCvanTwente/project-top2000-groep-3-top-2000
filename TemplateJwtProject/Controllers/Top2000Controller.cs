using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;
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
    private int CalculateTrend(int songId, int currentYear, IEnumerable<Top2000Entry> allEntries)
    {
        try
        {
            var currentEntry = allEntries.FirstOrDefault(t => t.SongId == songId && t.Year == currentYear);

            if (currentEntry == null) return 0;

            var previousYearEntry = allEntries.FirstOrDefault(t => t.SongId == songId && t.Year == currentYear - 1);

            if (previousYearEntry == null) return 0;

            // Trend: negative = went down, positive = went up
            return previousYearEntry.Position - currentEntry.Position;
        }
        catch
        (Exception ex)
        {
            Console.WriteLine($"Error calculating trend for SongId {songId} in year {currentYear}: {ex.Message}");
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
        try
        {
            var top10Entries = _context.Top2000Entries
                .Include(t => t.Song)
                    .ThenInclude(s => s!.Artist)
                .Where(t => t.Year == year)
                .OrderBy(t => t.Position)
                .Take(10)
                .ToList();

            // Load entries for trend calculation only for these song IDs
            var songIds = top10Entries.Select(t => t.SongId).ToList();
            var trendEntries = _context.Top2000Entries
                .Where(t => songIds.Contains(t.SongId) && (t.Year == year || t.Year == year - 1))
                .ToList();

            var top10 = top10Entries
                .Select(t => new Top2000EntryDto
                {
                    Position = t.Position,
                    Year = t.Year,
                    SongId = t.SongId,
                    Titel = t.Song!.Titel,
                    Artist = t.Song.Artist!.Name,
                    Trend = CalculateTrend(t.SongId, year, trendEntries)
                })
                .ToList();

            if (!top10.Any())
            {
                return NotFound(new { message = $"No Top 2000 entries found for year {year}" });
            }

            return Ok(top10);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching top 10 entries", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all entries from the Top 2000 for a specific year
    /// </summary>
    /// <param name="year">The year to retrieve Top 2000 entries for (default: 2024)</param>
    /// <returns>List of all entries for that year, sorted by position</returns>
    [HttpGet("by-year/{year}")]
    public IActionResult GetByYear(int year)
    {
        try
        {
            var yearEntries = _context.Top2000Entries
                .Include(t => t.Song)
                    .ThenInclude(s => s!.Artist)
                .Where(t => t.Year == year)
                .OrderBy(t => t.Position)
                .ToList();

            // Load entries for trend calculation only for these song IDs
            var songIds = yearEntries.Select(t => t.SongId).ToList();
            var trendEntries = _context.Top2000Entries
                .Where(t => songIds.Contains(t.SongId) && (t.Year == year || t.Year == year - 1))
                .ToList();

            var entries = yearEntries
                .Select(t => new Top2000EntryDto
                {
                    Position = t.Position,
                    Year = t.Year,
                    SongId = t.SongId,
                    Titel = t.Song!.Titel,
                    Artist = t.Song.Artist!.Name,
                    Trend = CalculateTrend(t.SongId, year, trendEntries)
                })
                .ToList();

            if (!entries.Any())
            {
                return NotFound(new { message = $"No Top 2000 entries found for year {year}" });
            }

            return Ok(entries);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while fetching entries for year {year}", error = ex.Message });
        }
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
        try
        {
            var positionEntry = _context.Top2000Entries
                .Include(t => t.Song)
                    .ThenInclude(s => s!.Artist)
                .FirstOrDefault(t => t.Position == position && t.Year == year);

            if (positionEntry == null)
            {
                return NotFound(new { message = $"No entry found at position {position} for year {year}" });
            }

            // Load entries for trend calculation only for this song ID
            var trendEntries = _context.Top2000Entries
                .Where(t => t.SongId == positionEntry.SongId && (t.Year == year || t.Year == year - 1))
                .ToList();

            var entry = new Top2000EntryDto
            {
                Position = positionEntry.Position,
                Year = positionEntry.Year,
                SongId = positionEntry.SongId,
                Titel = positionEntry.Song!.Titel,
                Artist = positionEntry.Song.Artist!.Name,
                Trend = CalculateTrend(positionEntry.SongId, year, trendEntries)
            };

            if (entry == null)
            {
                return NotFound(new { message = $"No entry found at position {position} for year {year}" });
            }

            return Ok(entry);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while fetching entry at position {position} for year {year}", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all Top 2000 entries for a specific song across all years
    /// </summary>
    /// <param name="songId">The song ID</param>
    /// <returns>List of all Top 2000 entries for that song, sorted by year descending</returns>
    [HttpGet("by-song/{songId}")]
    public IActionResult GetBySong(int songId)
    {
        try
        {
            var songEntries = _context.Top2000Entries
                .Include(t => t.Song)
                    .ThenInclude(s => s!.Artist)
                .Where(t => t.SongId == songId)
                .OrderByDescending(t => t.Year)
                .ToList();

            if (!songEntries.Any())
            {
                return NotFound(new { message = $"No Top 2000 entries found for song ID {songId}" });
            }

            var allEntries = _context.Top2000Entries.ToList();

            var entries = songEntries
                .Select(t => new Top2000EntryDto
                {
                    Position = t.Position,
                    Year = t.Year,
                    SongId = t.SongId,
                    Titel = t.Song!.Titel,
                    Artist = t.Song.Artist!.Name,
                    Trend = CalculateTrend(t.SongId, t.Year, allEntries)
                })
                .ToList();

            return Ok(entries);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while fetching entries for song ID {songId}", error = ex.Message });
        }
    }
}
