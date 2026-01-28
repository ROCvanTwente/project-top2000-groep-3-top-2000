using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class AddSongToPlaylistDto
{
    [Required(ErrorMessage = "Song ID is required")]
    public int SongId { get; set; }
}
