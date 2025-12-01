using UrlShortener.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Web.Persistence;

/// <summary>
/// Represents the Entity Framework Core database context for the application.
/// Contains DbSet properties for all persisted domain entities.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<UrlRecord> UrlRecords => Set<UrlRecord>();
    public DbSet<AboutContent> AboutContents => Set<AboutContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enforce unique email constraint
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Enforce unique URL and short code
        modelBuilder.Entity<UrlRecord>()
            .HasIndex(u => u.OriginalUrl)
            .IsUnique();

        modelBuilder.Entity<UrlRecord>()
            .HasIndex(u => u.ShortCode)
            .IsUnique();
        
        // Seed initial data to the DB
        SeedInitialData(modelBuilder);
    }
    
    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Admin",
                Email = "admin@example.com",
                PasswordHash = "cWk1XfCdVYjhua1t1cWObQ==:O13bIcnMg4zUyFhyF1PYgXWnWRPMbvpIjjWm5P/W5UA=",
                Role = "Admin"
            },
            new User
            {
                Id = 2,
                Name = "TestUser",
                Email = "user@example.com",
                PasswordHash = "R3oPf4sQD09/R5n2/0w3tg==:YCLcqto/32a/pXqtAI9E2y/Wu62xu7gJewrkl6f5RvA=",
                Role = "User"
            }
        );

        modelBuilder.Entity<AboutContent>().HasData(
            new AboutContent
            {
                Id = 1,
                Content = "Initial About Page Content...",
                LastUpdatedAtUtc = DateTime.UtcNow,
                UpdatedByUserId = 1
            }
        );
    }
}