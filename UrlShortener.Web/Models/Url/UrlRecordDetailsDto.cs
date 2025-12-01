namespace UrlShortener.Web.Models.Url;

/// <summary>
/// Represents a detailed view of a specific URL record,
/// displayed on the Short URL Info page.
/// </summary>
public class UrlRecordDetailsDto
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastAccessedAtUtc { get; set; }
    public int VisitCount { get; set; }
}