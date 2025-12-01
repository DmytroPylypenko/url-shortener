using UrlShortener.Web.Domain.Entities;
using UrlShortener.Web.Domain.Interfaces;

namespace UrlShortener.Web.Services.UrlShortening;

/// <summary>
/// Implements the business logic for generating short URLs,
/// validating input, ensuring uniqueness, and persisting records.
/// </summary>
public class UrlShorteningService : IUrlShorteningService
{
    private readonly IUrlRecordRepository _repository;
    private readonly IShortCodeGenerator _shortCodeGenerator;

    public UrlShorteningService(
        IUrlRecordRepository repository,
        IShortCodeGenerator shortCodeGenerator)
    {
        _repository = repository;
        _shortCodeGenerator = shortCodeGenerator;
    }

    public async Task<UrlRecord> CreateAsync(
        string originalUrl,
        int creatorUserId,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate URL format
        if (!Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
            throw new ArgumentException("Invalid URL format.", nameof(originalUrl));

        // 2. Check if URL already exists
        if (await _repository.UrlExistsAsync(originalUrl, cancellationToken))
            throw new InvalidOperationException("This URL already exists.");

        // 3. Generate unique short code
        string shortCode;
        do
        {
            shortCode = _shortCodeGenerator.Generate();
        }
        while (await _repository.ShortCodeExistsAsync(shortCode, cancellationToken));

        // 4. Create record
        var record = new UrlRecord
        {
            OriginalUrl = originalUrl,
            ShortCode = shortCode,
            CreatedByUserId = creatorUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(record, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return record;
    }
}