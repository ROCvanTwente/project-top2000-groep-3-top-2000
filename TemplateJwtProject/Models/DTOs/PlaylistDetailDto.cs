namespace TemplateJwtProject.Models.DTOs;

public class PlaylistDetailDto
{
    public int PlaylistId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PlaylistSongDto> Songs { get; set; } = new List<PlaylistSongDto>();
}
