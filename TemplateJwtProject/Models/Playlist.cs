namespace TemplateJwtProject.Models;

public class Playlist
{
    public int PlaylistId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ApplicationUser? User { get; set; }
    public ICollection<PlaylistSongs> PlaylistSongs { get; set; } = new List<PlaylistSongs>();
}
