using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Web.Domain.Interfaces;

namespace UrlShortener.Web.Controllers.Mvc;

/// <summary>
/// Handles rendering the view for detailed information about a single URL record.
/// This view is protected and requires user authentication.
/// </summary>
[Authorize]
[Route("[controller]")]
[ApiController]
public class InfoController : Controller
{
    private readonly IUrlRecordRepository _repository;
    
    public InfoController(IUrlRecordRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Fetches URL details and renders the Info view.
    /// </summary>
    /// <param name="id">The unique ID of the URL record.</param>
    /// /// <param name="token">Cancellation token.</param>
    /// <returns>A view displaying the detailed information.</returns>
    [HttpGet("urls/info/{id:int}")] // Defines a clear, specific route
    public async Task<IActionResult> Index(int id, CancellationToken token)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid URL ID specified.");
        }
        
        var record = await _repository.GetByIdAsync(id, token);

        if (record == null)
        {
            return NotFound();
        }
        
        return View(record); 
    }
}