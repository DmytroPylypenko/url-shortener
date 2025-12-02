using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UrlShortener.Web.Domain.Entities; 
using UrlShortener.Web.Services.Auth;   

namespace UrlShortener.Tests.Services.Auth;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var inMemoryConfig = new Dictionary<string, string>
        {
            { "JwtSettings:Key", "ThisIsMySuperSecureTestKeyForDotNetCore1234567890" },
            { "JwtSettings:Issuer", "https://test-issuer.com" },
            { "JwtSettings:Audience", "https://test-audience.com" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig!)
            .Build();

        _sut = new TokenService(configuration);
    }

    [Fact]
    public void CreateToken_WhenGivenUser_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new UrlShortener.Web.Domain.Entities.User
        {
            Id = 101,
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var tokenString = _sut.CreateToken(user);

        // Assert
        tokenString.Should().NotBeNullOrEmpty();
        tokenString.Should().Contain("."); // A JWT has two dots
    }

    [Fact]
    public void CreateToken_WhenGivenUser_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 101,
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var tokenString = _sut.CreateToken(user);

        // Assert - We need to decode the token to check its contents
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadJwtToken(tokenString);

        // Find the "sub" (Subject/UserID) claim
        var subClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(user.Id.ToString());

        // Find the "email" claim
        var emailClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(user.Email);
    }

    [Fact]
    public void CreateToken_ShouldBeSignedWithCorrectIssuerAndAudience()
    {
        // Arrange
        var user = new User { Id = 102, Email = "user2@example.com" };

        // Act
        var tokenString = _sut.CreateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadJwtToken(tokenString);

        decodedToken.Issuer.Should().Be("https://test-issuer.com");
        decodedToken.Audiences.First().Should().Be("https://test-audience.com");
    }
    
    [Fact]
    public void CreateToken_ShouldSetExpirationAndAlgorithmCorrectly()
    {
        // Arrange
        var user = new User { Id = 203, Email = "expire@test.com" };

        // Act
        var tokenString = _sut.CreateToken(user);
        var decodedToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

        // Assert
        decodedToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
        decodedToken.SignatureAlgorithm.Should().Be(SecurityAlgorithms.HmacSha256);
    }
    
    [Fact]
    public void Constructor_WhenJwtKeyIsMissing_ShouldThrow()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "JwtSettings:Issuer", "test" },
                { "JwtSettings:Audience", "test" }
                // Missing Key
            }!)
            .Build();
    
        // Act
        Action act = () => new TokenService(config);
    
        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT signing key is not configured.*");
    }
    
    [Fact]
    public void CreateToken_WhenUserIsNull_ShouldThrow()
    {
        // Act
        Action act = () => _sut.CreateToken(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("user");
    }
}