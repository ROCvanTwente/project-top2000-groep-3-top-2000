namespace TemplateJwtProject.Models;

public class PlaylistSongs
{
    public int PlaylistId { get; set; }
    public int SongId { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public Playlist? Playlist { get; set; }
    public Song? Song { get; set; }
}
