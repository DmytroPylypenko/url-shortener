using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces;

/// <summary>
/// Provides high-level operations for creating and validating short URLs.
/// </summary>
public interface IUrlShorteningService
{
    /// <summary>
    /// Creates a new <see cref="UrlRecord"/> if the URL does not already exist.
    /// Automatically generates a unique short code.
    /// </summary>
    /// <param name="originalUrl">The original full URL.</param>
    /// <param name="creatorUserId">User ID of the creator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The newly created <see cref="UrlRecord"/> instance.
    /// </returns>
    Task<UrlRecord> CreateAsync(
        string originalUrl,
        int creatorUserId,
        CancellationToken cancellationToken = default);
}