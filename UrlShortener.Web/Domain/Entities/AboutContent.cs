using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Web.Domain.Entities;

/// <summary>
/// Stores editable description of the URL shortening algorithm.
/// Only administrators may update it.
/// </summary>
public class AboutContent
{
    public int Id { get; set; }

    // Markdown or plain text content displayed on the About page.
    [Required]
    public string Content { get; set; } = string.Empty;

    // The timestamp when the content was last modified.
    public DateTime LastUpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Optional FK to the user who last updated the text.
    public int? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
}