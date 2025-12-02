using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces;


/// <summary>
/// Provides data-access operations for <see cref="User"/> entities,
/// including creation, lookup, and existence checks.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Checks whether a user with the specified email already exists.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>
    /// True if a user with this email exists; otherwise, false.
    /// </returns>
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    /// <param name="user">The user instance to add.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email of the user to find.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="User"/> instance if found; otherwise, null.
    /// </returns>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
}