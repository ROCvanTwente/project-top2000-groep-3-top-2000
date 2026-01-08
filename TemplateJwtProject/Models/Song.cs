namespace TemplateJwtProject.Models;

public class Song
{
    public int SongId { get; set; }
    public string Titel { get; set; } = string.Empty;
    
    // Foreign key
    public int ArtistId { get; set; }
    
    // Navigation properties
    public Artist? Artist { get; set; }
    public ICollection<Top2000Entry> Top2000Entries { get; set; } = new List<Top2000Entry>();
}
