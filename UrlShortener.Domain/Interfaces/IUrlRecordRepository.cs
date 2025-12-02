using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces;

/// <summary>
/// Provides data-access operations for <see cref="UrlRecord"/> entities,
/// including creation, lookup, deletion, and uniqueness checks.
/// </summary>
public interface IUrlRecordRepository
{
    /// <summary>
    /// Retrieves a <see cref="UrlRecord"/> by its unique database identifier.
    /// </summary>
    /// <param name="id">The record identifier.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>
    /// The matching <see cref="UrlRecord"/> instance, or null if no record exists.
    /// </returns>
    Task<UrlRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a <see cref="UrlRecord"/> by its short URL code (e.g. "aB91Z").
    /// </summary>
    /// <param name="shortCode">The short code to search for.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>
    /// A matching <see cref="UrlRecord"/> instance, or null if not found.
    /// </returns>
    Task<UrlRecord?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all existing URL records in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A collection of <see cref="UrlRecord"/> entities.
    /// </returns>
    Task<IEnumerable<UrlRecord>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new <see cref="UrlRecord"/> to the database.
    /// </summary>
    /// <param name="record">The instance to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(UrlRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified <see cref="UrlRecord"/> from the database.
    /// </summary>
    /// <param name="record">The record to remove.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    Task DeleteAsync(UrlRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a URL with the specified original full address already exists.
    /// </summary>
    /// <param name="originalUrl">The original URL to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// True if a record with this <paramref name="originalUrl"/> already exists; otherwise, false.
    /// </returns>
    Task<bool> UrlExistsAsync(string originalUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a short code already exists in the database.
    /// </summary>
    /// <param name="shortCode">The short code to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// True if the short code is already used; otherwise, false.
    /// </returns>
    Task<bool> ShortCodeExistsAsync(string shortCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists any pending changes to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}