using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Web.Models.Url;

namespace UrlShortener.Web.Controllers.Api;

/// <summary>
/// Exposes read-only endpoints for listing and viewing URL records.
/// Anonymous users may list URLs; details require authentication.
/// </summary>
[ApiController]
[Route("api/public/urls")]
public class PublicUrlsController : ControllerBase
{
    private readonly IUrlRecordRepository _repository;

    public PublicUrlsController(
        IUrlRecordRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a list of all URL records in the system.
    /// This endpoint is publicly accessible.
    /// </summary>
    /// <param name="token">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// HTTP 200 containing a list of <see cref="UrlRecordListItemDto"/> items.
    /// </returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        // 1. Fetch all records with necessary navigation properties.
        var list = await _repository.GetAllAsync(token);

        // 2. Project entities into DTOs for the Angular table view.
        var response = list.Select(r => new UrlRecordListItemDto
        {
            Id = r.Id,
            OriginalUrl = r.OriginalUrl,
            ShortCode = r.ShortCode,
            CreatedBy = r.CreatedByUser.Name,
            CreatedAtUtc = r.CreatedAtUtc,
            VisitCount = r.VisitCount
        });

        return Ok(response);
    }

    /// <summary>
    /// Retrieves detailed information about a specific URL record.
    /// Only authenticated users may access this endpoint.
    /// </summary>
    /// <param name="id">The unique identifier of the URL record.</param>
    /// <param name="token">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// <para><b>200 OK</b> with a <see cref="UrlRecordDetailsDto"/> if the record exists.</para>
    /// <para><b>401 Unauthorized</b> if the user is not authenticated.</para>
    /// <para><b>404 Not Found</b> if no URL record matches the provided ID.</para>
    /// </returns>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id, CancellationToken token)
    {
        // Ensure the request comes from an authenticated user (for tests)
        if (!User.Identity?.IsAuthenticated ?? false)
            return Unauthorized();
        
        // 1. Look up the record.
        var record = await _repository.GetByIdAsync(id, token);
        if (record is null)
            return NotFound();

        // 2. Convert to DTO.
        var dto = new UrlRecordDetailsDto
        {
            Id = record.Id,
            OriginalUrl = record.OriginalUrl,
            ShortCode = record.ShortCode,
            CreatedBy = record.CreatedByUser.Name,
            CreatedAtUtc = record.CreatedAtUtc,
            LastAccessedAtUtc = record.LastAccessedAtUtc,
            VisitCount = record.VisitCount
        };

        return Ok(dto);
    }
}