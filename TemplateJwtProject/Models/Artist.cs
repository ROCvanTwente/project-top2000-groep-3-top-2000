namespace TemplateJwtProject.Models;

public class Artist
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

    // Navigation property
    public ICollection<Song> Songs { get; set; } = new List<Song>();
}
