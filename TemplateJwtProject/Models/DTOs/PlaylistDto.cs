namespace TemplateJwtProject.Models.DTOs;

public class PlaylistDto
{
    public int PlaylistId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SongCount { get; set; }
}
