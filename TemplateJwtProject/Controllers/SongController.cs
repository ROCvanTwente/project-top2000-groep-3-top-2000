using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongController : ControllerBase
{
    private readonly AppDbContext _context;

    public SongController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all songs
    /// </summary>
    /// <returns>List of all songs</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Song>>> GetAllSongs()
    {
        try
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .ToListAsync();

            if (!songs.Any())
            {
                return NotFound(new { message = "No songs found" });
            }

            return Ok(songs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching all songs", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets songs with lyrics
    /// </summary>
    /// <returns>List of all songs that have lyrics</returns>
    [HttpGet("with-lyrics")]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongsWithLyrics()
    {
        try
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.Lyrics != null && s.Lyrics != "")
                .ToListAsync();

            if (!songs.Any())
            {
                return NotFound(new { message = "No songs with lyrics found" });
            }

            return Ok(songs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching songs with lyrics", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets songs with YouTube links
    /// </summary>
    /// <returns>List of all songs that have YouTube links</returns>
    [HttpGet("with-youtube")]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongsWithYoutube()
    {
        try
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.Youtube != null && s.Youtube != "")
                .ToListAsync();

            if (!songs.Any())
            {
                return NotFound(new { message = "No songs with YouTube links found" });
            }

            return Ok(songs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching songs with YouTube links", error = ex.Message });
        }
    }

    /// <summary>
    /// Searches for songs by title
    /// </summary>
    /// <param name="title">The song title or part of it</param>
    /// <returns>List of songs matching the search term</returns>
    [HttpGet("search/{title}")]
    public async Task<ActionResult<IEnumerable<Song>>> SearchSongs(string title)
    {
        try
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.Titel.Contains(title, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            if (!songs.Any())
            {
                return NotFound(new { message = $"No songs found matching '{title}'" });
            }

            return Ok(songs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while searching for songs matching '{title}'", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all songs by a specific artist
    /// </summary>
    /// <param name="artistId">The artist ID</param>
    /// <returns>List of songs by the artist</returns>
    [HttpGet("artist/{artistId}")]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongsByArtist(int artistId)
    {
        try
        {
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null)
            {
                return NotFound(new { message = $"Artist with ID {artistId} not found" });
            }

            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.ArtistId == artistId)
                .ToListAsync();

            if (!songs.Any())
            {
                return NotFound(new { message = $"No songs found for artist ID {artistId}" });
            }

            return Ok(songs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while fetching songs for artist ID {artistId}", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all songs released in a specific year
    /// </summary>
    /// <param name="year">The release year</param>
    /// <returns>List of songs released in that year</returns>
    [HttpGet("by-year/{year}")]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongsByYear(int year)
    {
        try
        {
            var yearDate = new DateTime(year, 1, 1);
            
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.ReleaseYear.HasValue && s.ReleaseYear.Value.Year == year)
                .ToListAsync();

            if (!songs.Any())
            {
                return NotFound(new { message = $"No songs found from year {year}" });
            }

            return Ok(songs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while fetching songs from year {year}", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific song by ID
    /// </summary>
    /// <param name="id">The song ID</param>
    /// <returns>Song details with artist information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Song>> GetSongById(int id)
    {
        try
        {
            var song = await _context.Songs
                .Include(s => s.Artist)
                .FirstOrDefaultAsync(s => s.SongId == id);

            if (song == null)
            {
                return NotFound(new { message = $"Song with ID {id} not found" });
            }

            return Ok(song);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while fetching song with ID {id}", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the lyrics for a specific song
    /// </summary>
    /// <param name="id">The song ID</param>
    /// <returns>Song lyrics if available</returns>
    [HttpGet("{id}/lyrics")]
    public async Task<ActionResult<object>> GetSongLyrics(int id)
    {
        try
        {
            var song = await _context.Songs.FirstOrDefaultAsync(s => s.SongId == id);

            if (song == null)
            {
                return NotFound(new { message = $"Song with ID {id} not found" });
            }

            if (string.IsNullOrEmpty(song.Lyrics))
            {
                return NotFound(new { message = $"No lyrics found for song ID {id}" });
            }

            return Ok(new { songId = song.SongId, titel = song.Titel, lyrics = song.Lyrics });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while fetching lyrics for song ID {id}", error = ex.Message });
        }
    }
}

