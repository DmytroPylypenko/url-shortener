using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;

namespace UrlShortener.Web.Controllers.Mvc;

/// <summary>
/// MVC Controller responsible for handling the "About" page, which contains the 
/// description of the shortening algorithm.
/// Access to view is public, but editing is restricted to Admin users.
/// </summary>
[Route("[controller]")]
public class AboutController : Controller
{
    private readonly IAboutContentRepository _repository;

    public AboutController(IAboutContentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Renders the About page content. Visible to all users (anonymous and authenticated).
    /// </summary>
    /// <param name="token">Cancellation token for the asynchronous operation.</param>
    /// <returns>A view displaying the algorithm description and an editor if the user is an Admin.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken token)
    {
        var content = await _repository.GetAsync(token);
        
        // Pass the content and a flag indicating if the current user is an Admin
        var isAdmin = User.IsInRole("Admin");
        
        ViewData["IsAdmin"] = isAdmin;
        
        return View(content);
    }

    /// <summary>
    /// Handles the form submission to update the About content. 
    /// This action is protected and requires the user to have the "Admin" role.
    /// </summary>
    /// <param name="model">The model containing the updated content text.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Redirects to the GET action upon successful update, or returns the view with errors.</returns>
    [Authorize(Roles = "Admin")] 
    [HttpPost]
    public async Task<IActionResult> Index([Bind("Content")] AboutContent model, CancellationToken token)
    {
        if (!ModelState.IsValid)
        {
            var existingContent = await _repository.GetAsync(token);
            ViewData["IsAdmin"] = true;
            return View(existingContent);
        }
        
        // 1. Get the single persistent record
        var content = await _repository.GetAsync(token);
        if (content == null)
        {
            return NotFound("About content record is missing.");
        }

        // 2. Update fields
        content.Content = model.Content;
        content.LastUpdatedAtUtc = DateTime.UtcNow;

        // Try to get the User ID from the claims 
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out var userId))
        {
            content.UpdatedByUserId = userId;
        }
        
        // 3. Save changes
        await _repository.UpdateAsync(content);
        await _repository.SaveChangesAsync(token);
        
        return RedirectToAction(nameof(Index));
    }
}