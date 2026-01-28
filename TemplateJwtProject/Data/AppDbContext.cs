using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Models;


namespace TemplateJwtProject.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Artist> Artists { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Top2000Entry> Top2000Entries { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistSongs> PlaylistSongs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // RefreshToken configuratie
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        // Artist configuratie - map to existing table
        builder.Entity<Artist>()
            .ToTable("Artist");

        // Song configuratie - map to existing table
        builder.Entity<Song>()
            .ToTable("Songs");
        
        builder.Entity<Song>()
            .HasOne(s => s.Artist)
            .WithMany(a => a.Songs)
            .HasForeignKey(s => s.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        // Top2000Entry configuratie - map to existing table
        builder.Entity<Top2000Entry>()
            .ToTable("Top2000Entries")
            .HasKey(t => new { t.SongId, t.Year });

        builder.Entity<Top2000Entry>()
            .HasOne(t => t.Song)
            .WithMany(s => s.Top2000Entries)
            .HasForeignKey(t => t.SongId)
            .OnDelete(DeleteBehavior.Restrict);

        // Playlist configuratie - map to existing table
        builder.Entity<Playlist>()
            .ToTable("Playlist");

        builder.Entity<Playlist>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // PlaylistSongs configuratie - map to existing table
        builder.Entity<PlaylistSongs>()
            .ToTable("PlaylistSongs")
            .HasKey(ps => new { ps.PlaylistId, ps.SongId });

        builder.Entity<PlaylistSongs>()
            .HasOne(ps => ps.Playlist)
            .WithMany(p => p.PlaylistSongs)
            .HasForeignKey(ps => ps.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlaylistSongs>()
            .HasOne(ps => ps.Song)
            .WithMany()
            .HasForeignKey(ps => ps.SongId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
