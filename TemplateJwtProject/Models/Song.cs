namespace TemplateJwtProject.Models;

public class Song
{
    public int SongId { get; set; }
    public string Titel { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? ImgUrl { get; set; }
    public string? Lyrics { get; set; }
    public string? Youtube { get; set; }
    
    // Foreign key
    public int ArtistId { get; set; }
    
    // Navigation properties
    public Artist? Artist { get; set; }
    public ICollection<Top2000Entry> Top2000Entries { get; set; } = new List<Top2000Entry>();
}
