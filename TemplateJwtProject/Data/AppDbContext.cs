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
    }
}
