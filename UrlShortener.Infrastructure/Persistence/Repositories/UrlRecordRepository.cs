using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;

namespace UrlShortener.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides Entity Framework Core–based data operations for
/// <see cref="UrlRecord"/> entities, including retrieval, creation,
/// deletion, and uniqueness checks.
/// </summary>
public class UrlRecordRepository : IUrlRecordRepository
{
    private readonly ApplicationDbContext _context;
    
    public UrlRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UrlRecord?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.UrlRecords
            .Include(r => r.CreatedByUser)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<UrlRecord?> GetByShortCodeAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.UrlRecords
            .Include(r => r.CreatedByUser)
            .FirstOrDefaultAsync(r => r.ShortCode == shortCode, cancellationToken);
    }

    public async Task<IEnumerable<UrlRecord>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.UrlRecords
            .Include(r => r.CreatedByUser)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        UrlRecord record,
        CancellationToken cancellationToken = default)
    {
        await _context.UrlRecords.AddAsync(record, cancellationToken);
    }

    public Task DeleteAsync(
        UrlRecord record,
        CancellationToken cancellationToken = default)
    {
        _context.UrlRecords.Remove(record);
        return Task.CompletedTask;
    }

    public async Task<bool> UrlExistsAsync(
        string originalUrl,
        CancellationToken cancellationToken = default)
    {
        return await _context.UrlRecords
            .AnyAsync(r => r.OriginalUrl == originalUrl, cancellationToken);
    }

    public async Task<bool> ShortCodeExistsAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.UrlRecords
            .AnyAsync(r => r.ShortCode == shortCode, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}