using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;

namespace TemplateJwtProject.Controllers;
//add playlist

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PlaylistController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlaylistController(AppDbContext context, ILogger<PlaylistController> logger, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    /// <summary>
    /// Gets the current authenticated user from the database
    /// </summary>
    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        // Try multiple ways to get the user ID (GUID)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid")
            ?? User.FindFirstValue("sub");

        if (!string.IsNullOrEmpty(userId))
        {
            // First try as GUID (user ID)
            if (Guid.TryParse(userId, out _))
            {
                var userById = await _userManager.FindByIdAsync(userId);
                if (userById != null)
                    return userById;
            }

            // Try as email
            var userByEmail = await _userManager.FindByEmailAsync(userId);
            if (userByEmail != null)
                return userByEmail;
        }

        // Try the email claim directly
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");

        if (!string.IsNullOrEmpty(email))
        {
            var userByEmail = await _userManager.FindByEmailAsync(email);
            if (userByEmail != null)
                return userByEmail;
        }

        return null;
    }

    /// <summary>
    /// Gets all playlists for the current user
    /// </summary>
    /// <returns>List of user's playlists</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlaylistDto>>> GetUserPlaylists()
    {
        try
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User not authenticated");
            }
            var userId = user.Id;

            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId)
                .Select(p => new PlaylistDto
                {
                    PlaylistId = p.PlaylistId,
                    UserId = p.UserId,
                    Name = p.Name,
                    CreatedAt = p.CreatedAt,
                    SongCount = p.PlaylistSongs.Count
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(playlists);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a specific playlist with all its songs
    /// </summary>
    /// <param name="id">Playlist ID</param>
    /// <returns>Playlist with songs</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<PlaylistDetailDto>> GetPlaylistById(int id)
    {
        try
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User not authenticated");
            }
            var userId = user.Id;

            var playlist = await _context.Playlists
                .Where(p => p.PlaylistId == id && p.UserId == userId)
                .Select(p => new PlaylistDetailDto
                {
                    PlaylistId = p.PlaylistId,
                    UserId = p.UserId,
                    Name = p.Name,
                    CreatedAt = p.CreatedAt,
                    Songs = p.PlaylistSongs
                        .Select(ps => new PlaylistSongDto
                        {
                            SongId = ps.SongId,
                            Titel = ps.Song!.Titel,
                            ArtistId = ps.Song.ArtistId,
                            ArtistName = ps.Song.Artist!.Name,
                            ReleaseYear = ps.Song.ReleaseYear,
                            ImgUrl = ps.Song.ImgUrl,
                            AddedAt = ps.AddedAt
                        })
                        .OrderBy(ps => ps.AddedAt)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (playlist == null)
            {
                return NotFound("Playlist not found or you don't have access to it");
            }

            return Ok(playlist);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new playlist
    /// </summary>
    /// <param name="createPlaylistDto">Playlist creation data</param>
    /// <returns>Created playlist</returns>
    [HttpPost]
    public async Task<ActionResult<PlaylistDto>> CreatePlaylist([FromBody] CreatePlaylistDto createPlaylistDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await GetCurrentUserAsync();
            _logger.LogInformation("CreatePlaylist: Current user retrieved: {UserId}", user?.Id);

            if (user == null)
            {
                _logger.LogWarning("CreatePlaylist: User not authenticated or not found in database");
                return Unauthorized("User not authenticated");
            }

            var userId = user.Id;

            var playlist = new Playlist
            {
                UserId = userId,
                Name = createPlaylistDto.Name,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("CreatePlaylist: Creating playlist with UserId: {UserId}, Name: {Name}", playlist.UserId, playlist.Name);

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            var playlistDto = new PlaylistDto
            {
                PlaylistId = playlist.PlaylistId,
                UserId = playlist.UserId,
                Name = playlist.Name,
                CreatedAt = playlist.CreatedAt,
                SongCount = 0
            };

            return CreatedAtAction(nameof(GetPlaylistById), new { id = playlist.PlaylistId }, playlistDto);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error when creating playlist");
            return StatusCode(500, $"Database error: {dbEx.InnerException?.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating playlist");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates a playlist name
    /// </summary>
    /// <param name="id">Playlist ID</param>
    /// <param name="updatePlaylistDto">Updated playlist data</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlaylist(int id, [FromBody] UpdatePlaylistDto updatePlaylistDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User not authenticated");
            }
            var userId = user.Id;

            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null)
            {
                return NotFound("Playlist not found or you don't have access to it");
            }

            playlist.Name = updatePlaylistDto.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a playlist
    /// </summary>
    /// <param name="id">Playlist ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlaylist(int id)
    {
        try
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User not authenticated");
            }
            var userId = user.Id;

            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null)
            {
                return NotFound("Playlist not found or you don't have access to it");
            }

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a song to a playlist
    /// </summary>
    /// <param name="id">Playlist ID</param>
    /// <param name="addSongDto">Song to add</param>
    /// <returns>No content</returns>
    [HttpPost("{id}/songs")]
    public async Task<IActionResult> AddSongToPlaylist(int id, [FromBody] AddSongToPlaylistDto addSongDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User not authenticated");
            }
            var userId = user.Id;

            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null)
            {
                return NotFound("Playlist not found or you don't have access to it");
            }

            // Check if song exists
            var songExists = await _context.Songs.AnyAsync(s => s.SongId == addSongDto.SongId);
            if (!songExists)
            {
                return NotFound("Song not found");
            }

            // Check if song is already in playlist
            var alreadyInPlaylist = await _context.PlaylistSongs
                .AnyAsync(ps => ps.PlaylistId == id && ps.SongId == addSongDto.SongId);

            if (alreadyInPlaylist)
            {
                return Conflict("Song is already in the playlist");
            }

            var playlistSong = new PlaylistSongs
            {
                PlaylistId = id,
                SongId = addSongDto.SongId,
                AddedAt = DateTime.UtcNow
            };

            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes a song from a playlist
    /// </summary>
    /// <param name="id">Playlist ID</param>
    /// <param name="songId">Song ID to remove</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}/songs/{songId}")]
    public async Task<IActionResult> RemoveSongFromPlaylist(int id, int songId)
    {
        try
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User not authenticated");
            }
            var userId = user.Id;

            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null)
            {
                return NotFound("Playlist not found or you don't have access to it");
            }

            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == id && ps.SongId == songId);

            if (playlistSong == null)
            {
                return NotFound("Song not found in this playlist");
            }

            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
