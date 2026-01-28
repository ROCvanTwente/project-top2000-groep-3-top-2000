namespace TemplateJwtProject.Models.DTOs;

/// <summary>
/// DTO for admin to update limited artist information.
/// Only Biography, Wiki (Wikipedia link), and Photo can be updated.
/// </summary>
public class AdminUpdateArtistDto
{
    /// <summary>
    /// Artist biography text
    /// </summary>
    public string? Biography { get; set; }

    /// <summary>
    /// Wikipedia link for the artist
    /// </summary>
    public string? Wiki { get; set; }

    /// <summary>
    /// Photo/image URL for the artist
    /// </summary>
    public string? Photo { get; set; }
}
