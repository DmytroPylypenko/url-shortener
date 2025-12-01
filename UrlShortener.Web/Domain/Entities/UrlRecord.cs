using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Web.Domain.Entities;

/// <summary>
/// Represents a shortened URL entry created by a user.
/// </summary>
public class UrlRecord
{
    public int Id { get; set; }
    
    // The original full URL to redirect to.
    [Required] 
    [Url] 
    [MaxLength(2048)] 
    public string OriginalUrl { get; set; } = string.Empty;

    // Short unique code generated via Base62
    [Required] 
    [MaxLength(16)] 
    public string ShortCode { get; set; } = string.Empty;

    // Foreign key reference to the creator of this record.
    [Required]
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    // The timestamp when the URL was created.
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    // When the URL was last accessed via redirect.
    public DateTime? LastAccessedAtUtc { get; set; }
    
    // Total number of successful redirects.
    [Range(0, int.MaxValue)]
    public int VisitCount { get; set; }
}