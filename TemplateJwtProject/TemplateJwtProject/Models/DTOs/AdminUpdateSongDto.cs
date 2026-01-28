namespace TemplateJwtProject.Models.DTOs;

/// <summary>
/// DTO for admin to update limited song information.
/// Only Lyrics, ImgUrl (album image), and Youtube link can be updated.
/// </summary>
public class AdminUpdateSongDto
{
    /// <summary>
    /// Song lyrics
    /// </summary>
    public string? Lyrics { get; set; }

    /// <summary>
    /// Album image URL
    /// </summary>
    public string? ImgUrl { get; set; }

    /// <summary>
    /// YouTube video link
    /// </summary>
    public string? Youtube { get; set; }
}
