using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Web.Domain.Interfaces;
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
    /// HTTP 200 with <see cref="UrlRecordDetailsDto"/> if found;
    /// HTTP 404 if not found.
    /// </returns>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id, CancellationToken token)
    {
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