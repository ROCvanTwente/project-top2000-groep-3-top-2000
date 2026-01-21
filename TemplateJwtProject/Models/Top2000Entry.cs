namespace TemplateJwtProject.Models;

public class Top2000Entry
{
    public int Position { get; set; }
    public int Year { get; set; }
    
    // Foreign key
    public int SongId { get; set; }
    
    // Navigation property
    public Song? Song { get; set; }
}
