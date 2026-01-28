using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class UpdatePlaylistDto
{
    [Required(ErrorMessage = "Playlist name is required")]
    [StringLength(200, ErrorMessage = "Playlist name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
}
