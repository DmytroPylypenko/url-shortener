using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Web.Domain.Interfaces;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers;

/// <summary>
/// Provides endpoints for authenticating users and issuing JWT tokens.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    
    public AuthController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Authenticates a user with email and password and returns a JWT on success.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>
    /// HTTP 200 with an <see cref="AuthResponseDto"/> on success,
    /// or HTTP 401 if credentials are invalid.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Lookup user by email
        var user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            // Return Unauthorized to prevent leaking info about which emails exist
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // 2. Verify password
        var isPasswordValid = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // 3. Generate JWT token
        var token = _tokenService.CreateToken(user);

        // 4. Return auth response
        var response = new AuthResponseDto
        {
            Token = token,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };

        return Ok(response);
    }
}
