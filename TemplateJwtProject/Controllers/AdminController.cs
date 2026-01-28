using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminController> _logger;
    private readonly AppDbContext? _context;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        ILogger<AdminController> logger,
        AppDbContext context)
    {
        _userManager = userManager;
        _logger = logger;
        _context = context;
    }

    // Added overload to preserve compatibility with tests that construct the controller
    // without providing an AppDbContext. _context will be null in that case.
    public AdminController(
        UserManager<ApplicationUser> userManager,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _logger = logger;
        _context = null;
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Valideer of de rol bestaat
        if (model.Role != Roles.Admin && model.Role != Roles.User)
        {
            return BadRequest(new { message = $"Invalid role. Valid roles are: {Roles.Admin}, {Roles.User}" });
        }

        // Check of gebruiker al deze rol heeft
        if (await _userManager.IsInRoleAsync(user, model.Role))
        {
            return BadRequest(new { message = $"User already has the {model.Role} role" });
        }

        var result = await _userManager.AddToRoleAsync(user, model.Role);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Failed to assign role", errors = result.Errors });
        }

        _logger.LogInformation("Admin assigned role {Role} to user {Email}", model.Role, model.Email);

        var roles = await _userManager.GetRolesAsync(user);
        
        return Ok(new 
        { 
            message = $"Role {model.Role} assigned successfully",
            email = user.Email,
            roles = roles
        });
    }

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!await _userManager.IsInRoleAsync(user, model.Role))
        {
            return BadRequest(new { message = $"User does not have the {model.Role} role" });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Failed to remove role", errors = result.Errors });
        }

        _logger.LogInformation("Admin removed role {Role} from user {Email}", model.Role, model.Email);

        var roles = await _userManager.GetRolesAsync(user);
        
        return Ok(new 
        { 
            message = $"Role {model.Role} removed successfully",
            email = user.Email,
            roles = roles
        });
    }

    [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
        
            var userList = new List<object>();
        
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    id = user.Id,
                    email = user.Email,
                    userName = user.UserName,
                    roles = roles
                });
            }

            return Ok(userList);
        }

        #region Artist Management (View All, View By ID, Update Extra Info Only)

        /// <summary>
        /// Gets all artists (Admin only)
        /// </summary>
        [HttpGet("artists")]
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
                        Photo = a.Photo
                    })
                    .ToListAsync();

                return Ok(artists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all artists");
                return StatusCode(500, new { message = "An error occurred while fetching artists", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific artist by ID (Admin only)
        /// </summary>
        [HttpGet("artists/{id}")]
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
                _logger.LogError(ex, "Error fetching artist with ID {ArtistId}", id);
                return StatusCode(500, new { message = $"An error occurred while fetching artist with ID {id}", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates extra information of an artist (Admin only).
        /// Can only update: Biography, Wiki (Wikipedia link), Photo/image.
        /// Cannot change: Artist name, add/delete artists.
        /// </summary>
        [HttpPut("artists/{id}")]
        public async Task<ActionResult<ArtistDto>> UpdateArtistInfo(int id, [FromBody] AdminUpdateArtistDto updateDto)
        {
            try
            {
                var artist = await _context.Artists.FindAsync(id);

                if (artist == null)
                {
                    return NotFound(new { message = $"Artist with ID {id} not found" });
                }

                if (updateDto.Biography != null)
                    artist.Biography = updateDto.Biography;
            
                if (updateDto.Wiki != null)
                    artist.Wiki = updateDto.Wiki;
            
                if (updateDto.Photo != null)
                    artist.Photo = updateDto.Photo;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin updated artist info for ArtistId {ArtistId}", id);

                var updatedArtist = new ArtistDto
                {
                    ArtistId = artist.ArtistId,
                    Name = artist.Name,
                    Wiki = artist.Wiki,
                    Biography = artist.Biography,
                    Photo = artist.Photo
                };

                return Ok(new 
                { 
                    message = "Artist information updated successfully",
                    artist = updatedArtist
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating artist info for ArtistId {ArtistId}", id);
                return StatusCode(500, new { message = $"An error occurred while updating artist with ID {id}", error = ex.Message });
            }
        }

        #endregion

        #region Song Management (View All, View By ID, Update Extra Info Only)

        /// <summary>
        /// Gets all songs (Admin only)
        /// </summary>
        [HttpGet("songs")]
        public async Task<ActionResult<IEnumerable<SongDto>>> GetAllSongs()
        {
            try
            {
                var songs = await _context.Songs
                    .Include(s => s.Artist)
                    .Select(s => new SongDto
                    {
                        SongId = s.SongId,
                        Titel = s.Titel,
                        ArtistId = s.ArtistId,
                        ArtistName = s.Artist!.Name,
                        ReleaseYear = s.ReleaseYear,
                        ImgUrl = s.ImgUrl,
                        Lyrics = s.Lyrics,
                        Youtube = s.Youtube
                    })
                    .ToListAsync();

                return Ok(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all songs");
                return StatusCode(500, new { message = "An error occurred while fetching songs", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific song by ID (Admin only)
        /// </summary>
        [HttpGet("songs/{id}")]
        public async Task<ActionResult<SongDto>> GetSongById(int id)
        {
            try
            {
                var song = await _context.Songs
                    .Include(s => s.Artist)
                    .Where(s => s.SongId == id)
                    .Select(s => new SongDto
                    {
                        SongId = s.SongId,
                        Titel = s.Titel,
                        ArtistId = s.ArtistId,
                        ArtistName = s.Artist!.Name,
                        ReleaseYear = s.ReleaseYear,
                        ImgUrl = s.ImgUrl,
                        Lyrics = s.Lyrics,
                        Youtube = s.Youtube
                    })
                    .FirstOrDefaultAsync();

                if (song == null)
                {
                    return NotFound(new { message = $"Song with ID {id} not found" });
                }

                return Ok(song);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching song with ID {SongId}", id);
                return StatusCode(500, new { message = $"An error occurred while fetching song with ID {id}", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates extra information of a song (Admin only).
        /// Can only update: Lyrics, ImgUrl (album image), Youtube link.
        /// Cannot change: Song title, artist, add/delete songs.
        /// </summary>
        [HttpPut("songs/{id}")]
        public async Task<ActionResult<SongDto>> UpdateSongInfo(int id, [FromBody] AdminUpdateSongDto updateDto)
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

                if (updateDto.Lyrics != null)
                    song.Lyrics = updateDto.Lyrics;
            
                if (updateDto.ImgUrl != null)
                    song.ImgUrl = updateDto.ImgUrl;
            
                if (updateDto.Youtube != null)
                    song.Youtube = updateDto.Youtube;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin updated song info for SongId {SongId}", id);

                var updatedSong = new SongDto
                {
                    SongId = song.SongId,
                    Titel = song.Titel,
                    ArtistId = song.ArtistId,
                    ArtistName = song.Artist?.Name ?? string.Empty,
                    ReleaseYear = song.ReleaseYear,
                    ImgUrl = song.ImgUrl,
                    Lyrics = song.Lyrics,
                    Youtube = song.Youtube
                };

                return Ok(new 
                { 
                    message = "Song information updated successfully",
                    song = updatedSong
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating song info for SongId {SongId}", id);
                return StatusCode(500, new { message = $"An error occurred while updating song with ID {id}", error = ex.Message });
            }
        }

        #endregion

        #region Top2000 Management (Read-Only)

        /// <summary>
        /// Gets the top 10 songs from the Top 2000 for a specific year (Admin only, read-only)
        /// </summary>
        [HttpGet("top2000/top10")]
        public async Task<IActionResult> GetTop2000Top10(int year = 2024)
        {
            try
            {
                var top10 = await _context.Top2000Entries
                    .Include(t => t.Song)
                        .ThenInclude(s => s!.Artist)
                    .Where(t => t.Year == year)
                    .OrderBy(t => t.Position)
                    .Take(10)
                    .Select(t => new Top2000EntryDto
                    {
                        Position = t.Position,
                        Year = t.Year,
                        SongId = t.SongId,
                        Titel = t.Song!.Titel,
                        Artist = t.Song.Artist!.Name,
                        Trend = 0
                    })
                    .ToListAsync();

                if (!top10.Any())
                {
                    return NotFound(new { message = $"No Top 2000 entries found for year {year}" });
                }

                return Ok(top10);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Top 2000 top 10 for year {Year}", year);
                return StatusCode(500, new { message = "An error occurred while fetching top 10 entries", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all entries from the Top 2000 for a specific year (Admin only, read-only)
        /// </summary>
        [HttpGet("top2000/by-year/{year}")]
        public async Task<IActionResult> GetTop2000ByYear(int year)
        {
            try
            {
                var entries = await _context.Top2000Entries
                    .Include(t => t.Song)
                        .ThenInclude(s => s!.Artist)
                    .Where(t => t.Year == year)
                    .OrderBy(t => t.Position)
                    .Select(t => new Top2000EntryDto
                    {
                        Position = t.Position,
                        Year = t.Year,
                        SongId = t.SongId,
                        Titel = t.Song!.Titel,
                        Artist = t.Song.Artist!.Name,
                        Trend = 0
                    })
                    .ToListAsync();

                if (!entries.Any())
                {
                    return NotFound(new { message = $"No Top 2000 entries found for year {year}" });
                }

                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Top 2000 entries for year {Year}", year);
                return StatusCode(500, new { message = $"An error occurred while fetching entries for year {year}", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all available years in the Top 2000 data (Admin only, read-only)
        /// </summary>
        [HttpGet("top2000/years")]
        public async Task<IActionResult> GetTop2000Years()
        {
            try
            {
                var years = await _context.Top2000Entries
                    .Select(t => t.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                if (!years.Any())
                {
                    return NotFound(new { message = "No Top 2000 data available" });
                }

                return Ok(years);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Top 2000 available years");
                return StatusCode(500, new { message = "An error occurred while fetching available years", error = ex.Message });
            }
        }

        #endregion
    }
