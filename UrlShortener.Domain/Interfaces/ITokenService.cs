using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces;

/// <summary>
/// Defines the contract for a service that creates JWT tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a JWT for a given user.
    /// </summary>
    /// <param name="user">The user for whom to create the token.</param>
    /// <returns>A signed JWT string.</returns>
    string CreateToken(User user);
}