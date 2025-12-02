using Microsoft.AspNetCore.Mvc;
using UrlShortener.Web.Domain.Interfaces;

namespace UrlShortener.Web.Controllers.Mvc;

/// <summary>
/// Handles redirection from a short code to the original URL.
/// Publicly accessible to everyone.
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
[Route("")]
public class RedirectController : Controller
{
    private readonly IUrlRecordRepository _repository;

    public RedirectController(IUrlRecordRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Redirects the client to the original URL that corresponds
    /// to the provided short code.
    /// </summary>
    /// <param name="shortCode">The unique short identifier (Base62).</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>
    /// HTTP 302 redirect if found; HTTP 404 otherwise.
    /// </returns>
    [HttpGet("r/{shortCode}")]
    public async Task<IActionResult> RedirectToOriginal(
        string shortCode,
        CancellationToken token)
    {
        // 1. Find record by short code.
        var record = await _repository.GetByShortCodeAsync(shortCode, token);
        if (record is null)
            return NotFound("Short URL not found.");

        // 2. Update access statistics.
        record.VisitCount++;
        record.LastAccessedAtUtc = DateTime.UtcNow;

        await _repository.SaveChangesAsync(token);

        // 3. Redirect user to the original URL.
        return Redirect(record.OriginalUrl);
    }
}