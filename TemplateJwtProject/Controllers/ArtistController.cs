using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistController : ControllerBase
{
    private readonly AppDbContext _context;

    public ArtistController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all artists
    /// </summary>
    /// <returns>List of all artists</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Artist>>> GetAllArtists()
    {
        var artists = await _context.Artists.ToListAsync();

        if (!artists.Any())
        {
            return NotFound(new { message = "No artists found" });
        }

        return Ok(artists);
    }

    /// <summary>
    /// Gets a specific artist by ID
    /// </summary>
    /// <param name="id">The artist ID</param>
    /// <returns>Artist details with their songs</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Artist>> GetArtistById(int id)
    {
        var artist = await _context.Artists
            .Include(a => a.Songs)
            .FirstOrDefaultAsync(a => a.ArtistId == id);

        if (artist == null)
        {
            return NotFound(new { message = $"Artist with ID {id} not found" });
        }

        return Ok(artist);
    }

    /// <summary>
    /// Searches for artists by name
    /// </summary>
    /// <param name="name">The artist name or part of it</param>
    /// <returns>List of artists matching the search term</returns>
    [HttpGet("search/{name}")]
    public async Task<ActionResult<IEnumerable<Artist>>> SearchArtists(string name)
    {
        var artists = await _context.Artists
            .Where(a => a.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();

        if (!artists.Any())
        {
            return NotFound(new { message = $"No artists found matching '{name}'" });
        }

        return Ok(artists);
    }

    /// <summary>
    /// Gets all songs by a specific artist
    /// </summary>
    /// <param name="artistId">The artist ID</param>
    /// <returns>List of songs by the artist</returns>
    [HttpGet("{artistId}/songs")]
    public async Task<ActionResult<IEnumerable<Song>>> GetArtistSongs(int artistId)
    {
        var artist = await _context.Artists.FindAsync(artistId);
        if (artist == null)
        {
            return NotFound(new { message = $"Artist with ID {artistId} not found" });
        }

        var songs = await _context.Songs
            .Where(s => s.ArtistId == artistId)
            .ToListAsync();

        if (!songs.Any())
        {
            return NotFound(new { message = $"No songs found for artist ID {artistId}" });
        }

        return Ok(songs);
    }
}
