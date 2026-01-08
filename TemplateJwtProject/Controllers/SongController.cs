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
        var songs = await _context.Songs
            .Include(s => s.Artist)
            .ToListAsync();

        if (!songs.Any())
        {
            return NotFound(new { message = "No songs found" });
        }

        return Ok(songs);
    }

    /// <summary>
    /// Gets a specific song by ID
    /// </summary>
    /// <param name="id">The song ID</param>
    /// <returns>Song details with artist information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Song>> GetSongById(int id)
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

    /// <summary>
    /// Searches for songs by title
    /// </summary>
    /// <param name="title">The song title or part of it</param>
    /// <returns>List of songs matching the search term</returns>
    [HttpGet("search/{title}")]
    public async Task<ActionResult<IEnumerable<Song>>> SearchSongs(string title)
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

    /// <summary>
    /// Gets all songs by a specific artist
    /// </summary>
    /// <param name="artistId">The artist ID</param>
    /// <returns>List of songs by the artist</returns>
    [HttpGet("artist/{artistId}")]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongsByArtist(int artistId)
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

    /// <summary>
    /// Gets all songs released in a specific year
    /// </summary>
    /// <param name="year">The release year</param>
    /// <returns>List of songs released in that year</returns>
    [HttpGet("by-year/{year}")]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongsByYear(int year)
    {
        var songs = await _context.Songs
            .Include(s => s.Artist)
            .Where(s => s.ReleaseYear == year)
            .ToListAsync();

        if (!songs.Any())
        {
            return NotFound(new { message = $"No songs found from year {year}" });
        }

        return Ok(songs);
    }

    /// <summary>
    /// Gets songs with lyrics
    /// </summary>
    /// <returns>List of all songs that have lyrics</returns>
    [HttpGet("with-lyrics")]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongsWithLyrics()
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

    /// <summary>
    /// Gets songs with YouTube links
    /// </summary>
    /// <returns>List of all songs that have YouTube links</returns>
    [HttpGet("with-youtube")]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongsWithYoutube()
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

    /// <summary>
    /// Gets the lyrics for a specific song
    /// </summary>
    /// <param name="id">The song ID</param>
    /// <returns>Song lyrics if available</returns>
    [HttpGet("{id}/lyrics")]
    public async Task<ActionResult<object>> GetSongLyrics(int id)
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
}
