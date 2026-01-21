namespace TemplateJwtProject.Models.DTOs;

public class ArtistDto
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Wiki { get; set; }
    public string? Biography { get; set; }
    public string? Photo { get; set; }
}

public class ArtistWithSongsDto
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Wiki { get; set; }
    public string? Biography { get; set; }
    public string? Photo { get; set; }
    public List<SongDto> Songs { get; set; } = new List<SongDto>();
}
