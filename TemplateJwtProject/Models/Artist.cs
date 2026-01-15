namespace TemplateJwtProject.Models;

public class Artist
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Song> Songs { get; set; } = new List<Song>();
}
