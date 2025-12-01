namespace UrlShortener.Web.Domain.Interfaces;

/// <summary>
/// Defines an algorithm for generating short alphanumeric URL codes.
/// </summary>
public interface IShortCodeGenerator
{
    /// <summary>
    /// Generates a new unique short string identifier (e.g. "aB93f").
    /// </summary>
    /// <returns>A compact, URL-safe short code.</returns>
    string Generate();
}