using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UrlShortener.Web.Configuration;
using UrlShortener.Web.Models.Auth;

namespace UrlShortener.Web.Controllers.Mvc;

/// <summary>
/// Provides endpoints for user login and logout using a Razor form.
/// Handles authentication by calling the API and storing the JWT token
/// inside a secure HTTP-only cookie.
/// </summary>
public class LoginController : Controller
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ApiSettings _apiSettings;
    
    public LoginController(IHttpClientFactory clientFactory, IOptions<ApiSettings> apiOptions)
    {
        _clientFactory = clientFactory;
        _apiSettings = apiOptions.Value;
    }

    /// <summary>
    /// Displays the login page to the user.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View(new LoginViewModel());
    }

    /// <summary>
    /// Processes the submitted login form, sends credentials to the API,
    /// and stores the returned JWT token inside an HttpOnly cookie.
    /// </summary>
    /// <param name="model">The login data entered by the user (email and password).</param>
    /// <returns>
    /// Redirects the user to the home page on successful login.
    /// Returns the login view again with an error message on failure.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        // 1. Validate incoming data.
        if (!ModelState.IsValid)
            return View(model);

        // 2. Create HttpClient for API communication.
        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri(_apiSettings.BaseUrl);

        // 3. Send login request to the API.
        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            model.Email,
            model.Password
        });

        // 4. If login failed, return form with error message.
        if (!response.IsSuccessStatusCode)
        {
            model.ErrorMessage = "Invalid email or password.";
            return View(model);
        }

        // 5. Read JWT token from API response.
        var json = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        string token = json!["token"];

        // 6. Store token securely in an HTTP-only cookie.
        Response.Cookies.Append(
            "auth_token",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(7)
            });

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Logs the user out by removing the cookie.
    /// </summary>
    [HttpGet]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return RedirectToAction("Index");
    }
}