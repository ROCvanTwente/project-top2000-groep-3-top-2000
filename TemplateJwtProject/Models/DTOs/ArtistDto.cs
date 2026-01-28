namespace TemplateJwtProject.Models.DTOs;

public class ArtistDto
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Wiki { get; set; }
    public string? Biography { get; set; }
    public string? Photo { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? Thumbnail { get; set; }
    public string? Style { get; set; }
    public string? Genre { get; set; }
    public int? FormedYear { get; set; }
    public int? Members { get; set; }
}

public class ArtistWithSongsDto
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Wiki { get; set; }
    public string? Biography { get; set; }
    public string? Photo { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? Thumbnail { get; set; }
    public string? Style { get; set; }
    public string? Genre { get; set; }
    public int? FormedYear { get; set; }
    public int? Members { get; set; }
    public List<SongDto> Songs { get; set; } = new List<SongDto>();
}
