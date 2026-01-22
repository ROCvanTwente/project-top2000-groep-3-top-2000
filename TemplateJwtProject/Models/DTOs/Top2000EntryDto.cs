namespace TemplateJwtProject.Models.DTOs;

public class Top2000EntryDto
{
    public int Position { get; set; }
    public int Year { get; set; }
    public int SongId { get; set; }
    public string Titel { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Trend { get; set; } = 0;
}
