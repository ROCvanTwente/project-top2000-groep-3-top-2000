namespace TemplateJwtProject.Models;

public class Artist
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Wiki { get; set; }
    public string? Biography { get; set; }
    public string? Photo { get; set; }

    // Navigation property
    public ICollection<Song> Songs { get; set; } = new List<Song>();
}
