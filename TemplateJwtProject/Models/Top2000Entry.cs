namespace TemplateJwtProject.Models;

public class Top2000Entry
{
    // Composite primary key (SongId, Year)
    public int SongId { get; set; }
    public DateTime Year { get; set; }
    public int Position { get; set; }
    
    // Navigation property
    public Song? Song { get; set; }
}
