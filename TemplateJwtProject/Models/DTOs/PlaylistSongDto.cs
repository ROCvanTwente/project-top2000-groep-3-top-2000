namespace TemplateJwtProject.Models.DTOs;

public class PlaylistSongDto
{
    public int SongId { get; set; }
    public string Titel { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? ImgUrl { get; set; }
    public DateTime AddedAt { get; set; }
}
