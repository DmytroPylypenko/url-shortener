namespace UrlShortener.Web.Models.Url;

/// <summary>
/// Represents the data returned to the client after a new shortened URL
/// has been successfully created.
/// </summary>
public class CreateShortUrlResponseDto
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
}