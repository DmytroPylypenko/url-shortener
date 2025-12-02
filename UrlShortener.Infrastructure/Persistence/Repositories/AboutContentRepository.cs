using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;

namespace UrlShortener.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides Entity Framework Core–based data operations for
/// the single <see cref="AboutContent"/> entity, handling its retrieval,
/// creation (if missing), and updates.
/// </summary>
public class AboutContentRepository : IAboutContentRepository
{
    private readonly ApplicationDbContext _context;

    public AboutContentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AboutContent?> GetAsync(CancellationToken cancellationToken)
    {
        var content = await _context.AboutContents.FirstOrDefaultAsync(cancellationToken);
        
        if (content == null)
        {
            content = new AboutContent
            {
                Content = "Welcome! This URL shortener uses Base62 encoding to generate compact short codes. This ensures a large pool of unique, case-sensitive identifiers (0-9, a-z, A-Z) that are URL-safe.",
                LastUpdatedAtUtc = DateTime.UtcNow
            };
            await _context.AboutContents.AddAsync(content, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return content;
    }

    public Task UpdateAsync(AboutContent content)
    {
        _context.AboutContents.Update(content);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}