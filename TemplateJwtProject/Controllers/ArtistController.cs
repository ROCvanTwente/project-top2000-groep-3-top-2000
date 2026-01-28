using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;

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
    public async Task<ActionResult<IEnumerable<ArtistDto>>> GetAllArtists()
    {
        try
        {
            var artists = await _context.Artists
                .Select(a => new ArtistDto
                {
                    ArtistId = a.ArtistId,
                    Name = a.Name,
                    Wiki = a.Wiki,
                    Biography = a.Biography,
                    Photo = a.Photo,
                    Country = a.Country,
                    CountryCode = a.CountryCode,
                    Thumbnail = a.Thumbnail,
                    Style = a.Style,
                    Genre = a.Genre,
                    FormedYear = a.FormedYear,
                    Members = a.Members
                })
                .ToListAsync();

            if (!artists.Any())
            {
                return NotFound(new { message = "No artists found" });
            }

            return Ok(artists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching all artists", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific artist by ID
    /// </summary>
    /// <param name="id">The artist ID</param>
    /// <returns>Artist details with their songs</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ArtistWithSongsDto>> GetArtistById(int id)
    {
        try
        {
            var artist = await _context.Artists
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.ArtistId == id);

            if (artist == null)
            {
                return NotFound(new { message = $"Artist with ID {id} not found" });
            }

            var artistDto = new ArtistWithSongsDto
            {
                ArtistId = artist.ArtistId,
                Name = artist.Name,
                Wiki = artist.Wiki,
                Biography = artist.Biography,
                Photo = artist.Photo,
                Country = artist.Country,
                CountryCode = artist.CountryCode,
                Thumbnail = artist.Thumbnail,
                Style = artist.Style,
                Genre = artist.Genre,
                FormedYear = artist.FormedYear,
                Members = artist.Members,
                Songs = artist.Songs.Select(s => new SongDto
                {
                    SongId = s.SongId,
                    Titel = s.Titel,
                    ReleaseYear = s.ReleaseYear,
                    ImgUrl = s.ImgUrl,
                    Lyrics = s.Lyrics,
                    Youtube = s.Youtube,
                    ArtistId = s.ArtistId,
                    ArtistName = artist.Name
                }).ToList()
            };

            return Ok(artistDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while fetching artist with ID {id}", error = ex.Message });
        }
    }

    /// <summary>
    /// Searches for artists by name
    /// </summary>
    /// <param name="name">The artist name or part of it</param>
    /// <returns>List of artists matching the search term</returns>
    [HttpGet("search/{name}")]
    public async Task<ActionResult<IEnumerable<ArtistDto>>> SearchArtists(string name)
    {
        try
        {
            var artists = await _context.Artists
                .Where(a => a.Name.Contains(name))
                .Select(a => new ArtistDto
                {
                    ArtistId = a.ArtistId,
                    Name = a.Name,
                    Wiki = a.Wiki,
                    Biography = a.Biography,
                    Photo = a.Photo,
                    Country = a.Country,
                    CountryCode = a.CountryCode,
                    Thumbnail = a.Thumbnail,
                    Style = a.Style,
                    Genre = a.Genre,
                    FormedYear = a.FormedYear,
                    Members = a.Members
                })
                .ToListAsync();

            if (!artists.Any())
            {
                return NotFound(new { message = $"No artists found matching '{name}'" });
            }

            return Ok(artists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while searching for artists matching '{name}'", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all songs by a specific artist
    /// </summary>
    /// <param name="artistId">The artist ID</param>
    /// <returns>List of songs by the artist</returns>
    [HttpGet("{artistId}/songs")]
    public async Task<ActionResult<IEnumerable<SongDto>>> GetArtistSongs(int artistId)
    {
        try
        {
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null)
            {
                return NotFound(new { message = $"Artist with ID {artistId} not found" });
            }

            var songs = await _context.Songs
                .Where(s => s.ArtistId == artistId)
                .Select(s => new SongDto
                {
                    SongId = s.SongId,
                    Titel = s.Titel,
                    ReleaseYear = s.ReleaseYear,
                    ImgUrl = s.ImgUrl,
                    Lyrics = s.Lyrics,
                    Youtube = s.Youtube,
                    ArtistId = s.ArtistId,
                    ArtistName = artist.Name
                })
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
}
