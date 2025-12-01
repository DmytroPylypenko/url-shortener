namespace UrlShortener.Web.Models;

/// <summary>
/// Represents the result of a successful authentication request.
/// </summary>
public class AuthResponseDto
{
    // The issued JWT access token.
    public string Token { get; set; } = string.Empty;
        
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Role { get; set; } = string.Empty;
}