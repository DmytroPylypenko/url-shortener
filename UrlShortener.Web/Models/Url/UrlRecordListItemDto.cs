namespace UrlShortener.Web.Models.Url;

/// <summary>
/// Represents a simplified projection of a URL record,
/// intended for display in the Angular Short URLs table.
/// </summary>
public class UrlRecordListItemDto
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int VisitCount { get; set; }
}