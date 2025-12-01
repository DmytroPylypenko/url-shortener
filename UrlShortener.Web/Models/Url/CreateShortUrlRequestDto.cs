using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Web.Models.Url;

/// <summary>
/// Represents the incoming payload for creating a new shortened URL.
/// Contains only the data required from the client.
/// </summary>
public class CreateShortUrlRequestDto
{
    [Required]
    [MaxLength(2048)]
    public string OriginalUrl { get; set; } = string.Empty;
}