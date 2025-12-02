using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Web.Models.Url;

namespace UrlShortener.Web.Controllers.Api;

/// <summary>
/// Provides authenticated endpoints for creating and deleting URL records.
/// Regular users may modify only their own records.
/// Admin users may modify all records.
/// </summary>
[ApiController]
[Route("api/manage/urls")]
[Authorize]
public class ManageUrlsController : ControllerBase
{
    private readonly IUrlRecordRepository _repository;
    private readonly IUrlShorteningService _shorteningService;

    public ManageUrlsController(
        IUrlRecordRepository repository,
        IUrlShorteningService shorteningService)
    {
        _repository = repository;
        _shorteningService = shorteningService;
    }
    
    /// <summary>
    /// Creates a new shortened URL for the authenticated user.
    /// </summary>
    /// <param name="request">The incoming request containing the original long URL.</param>
    /// <param name="token">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// HTTP 200 with <see cref="CreateShortUrlResponseDto"/> on success;
    /// HTTP 400 if validation fails; HTTP 401 if user is unauthenticated.
    /// </returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateShortUrlRequestDto request,
        CancellationToken token)
    {
        // 1. Validate request model.
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 2. Extract the user ID from JWT claims.
        string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId)) 
            return Unauthorized(new { message = "You are not logged in.. Please log in to shorten URLs." });
        
        // 3. Delegate business logic to the shortening service.
        var record = await _shorteningService.CreateAsync(
            request.OriginalUrl,
            userId,
            token);

        // 4. Prepare response DTO.
        var response = new CreateShortUrlResponseDto
        {
            Id = record.Id,
            OriginalUrl = record.OriginalUrl,
            ShortCode = record.ShortCode
        };

        return Ok(response);
    }

    /// <summary>
    /// Deletes a URL record. Regular users may delete only their own URLs.
    /// Admin users may delete any URL.
    /// </summary>
    /// <param name="id">The ID of the URL record to delete.</param>
    /// <param name="token">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// HTTP 204 on success; HTTP 404 if not found;
    /// HTTP 403 if user lacks permission.
    /// </returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        // 1. Fetch the record.
        var record = await _repository.GetByIdAsync(id, token);
        if (record is null)
            return NotFound();

        // 2. Extract role and user ID from JWT.
        string role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId)) 
            return Unauthorized();

        // 3. Authorization check
        if (role != "Admin" && record.CreatedByUserId != userId)
            return Forbid();

        // 4. Perform deletion.
        await _repository.DeleteAsync(record, token);
        await _repository.SaveChangesAsync(token);

        return NoContent();
    }
}