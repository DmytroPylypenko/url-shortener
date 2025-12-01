namespace UrlShortener.Web.Domain.Interfaces;

/// <summary>
/// Defines methods for password hashing and verification services.
/// Essential for testability and abstraction of the hashing algorithm.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password for secure storage.
    /// </summary>
    /// <param name="password">The password string to hash.</param>
    /// <returns>A string containing the salt and hash, separated by a delimiter.</returns>
    string Hash(string password);
    
    /// <summary>
    /// Verifies a plain-text password against a stored password hash.
    /// </summary>
    /// <param name="passwordHash">The stored hash string (including salt).</param>
    /// <param name="password">The plain-text password provided by the user.</param>
    /// <returns>True if the password matches the stored hash; otherwise, false.</returns>
    bool Verify(string passwordHash, string password);
}