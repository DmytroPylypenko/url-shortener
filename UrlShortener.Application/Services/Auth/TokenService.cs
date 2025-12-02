using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;

namespace UrlShortener.Application.Services.Auth;

/// <summary>
/// Implements the ITokenService to generate JWT tokens.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _signingKey;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        string? key = _configuration["JwtSettings:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured. Please set 'JwtSettings:Key' in configuration.");
        }

        // Convert the configured secret key into a symmetric security key for signing tokens.
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
    
    public string CreateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        // 1. Define the user's claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        // 2. Create signing credentials
        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature);

        // 3. Describe the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7), 
            SigningCredentials = credentials,
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"]
        };

        // 4. Create and write the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}