
using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces;

/// <summary>
/// Defines data access operations for the static <see cref="AboutContent"/> record,
/// which stores the editable algorithm description.
/// </summary>
public interface IAboutContentRepository
{
    /// <summary>
    /// Retrieves the single <see cref="AboutContent"/> record from the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>
    /// The <see cref="AboutContent"/> instance if found; otherwise, null.
    /// </returns>
    Task<AboutContent?> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Marks the existing <see cref="AboutContent"/> record for update.
    /// </summary>
    /// <param name="content">The modified content entity.</param>
    Task UpdateAsync(AboutContent content);
    
    /// <summary>
    /// Persists any pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}