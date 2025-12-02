using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Web.Controllers.Mvc;

/// <summary>
/// Serves the main entry point for the Angular client application.
/// This controller acts as a container to host the Single Page Application (SPA)
/// within the ASP.NET Core MVC layout.
/// </summary>
public class UrlsController : Controller
{
    /// <summary>
    /// Renders the Razor view that contains the Angular root component.
    /// This allows the Angular app to bootstrap and take over routing on the client side.
    /// </summary>
    /// <returns>The view containing the Angular application scripts and styles.</returns>
    public IActionResult Index()
    {
        return View();
    }
}