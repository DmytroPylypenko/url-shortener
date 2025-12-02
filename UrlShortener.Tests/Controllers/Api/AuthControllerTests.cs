using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UrlShortener.Web.Controllers.Api;
using UrlShortener.Web.Domain.Entities;
using UrlShortener.Web.Domain.Interfaces;
using UrlShortener.Web.Models;

namespace UrlShortener.Tests.Controllers.Api;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ITokenService> _mockTokenService;

    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenService = new Mock<ITokenService>();

        _sut = new AuthController(
            _mockUserRepo.Object,
            _mockPasswordHasher.Object,
            _mockTokenService.Object);
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------
    private void SetAuthenticatedUser()
    {
        var claims = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "1")],
            "TestAuth");

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(claims)
            }
        };
    }

    private void SetAnonymousUser()
    {
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ------------------------------------------------------------
    // LOGIN
    // ------------------------------------------------------------
    [Fact]
    public async Task Login_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        _sut.ModelState.AddModelError("Email", "Required");

        var dto = new LoginRequestDto();

        // Act
        var result = await _sut.Login(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WhenEmailNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "a@a.com", Password = "123" };

        _mockUserRepo
            .Setup(r => r.FindByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Login(dto, CancellationToken.None);

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WhenPasswordIncorrect_ShouldReturnUnauthorized()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "a@a.com", Password = "wrong" };

        var user = new User
        {
            Id = 1,
            Email = dto.Email,
            Name = "Dima",
            Role = "User",
            PasswordHash = "HASH"
        };

        _mockUserRepo
            .Setup(r => r.FindByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(h => h.Verify(user.PasswordHash, dto.Password))
            .Returns(false);

        // Act
        var result = await _sut.Login(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ShouldReturnJwtToken()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "dima@test.com",
            Password = "correct"
        };

        var user = new User
        {
            Id = 99,
            Email = dto.Email,
            Name = "Dima",
            Role = "Admin",
            PasswordHash = "HASH"
        };

        _mockUserRepo
            .Setup(r => r.FindByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(h => h.Verify(user.PasswordHash, dto.Password))
            .Returns(true);

        _mockTokenService
            .Setup(t => t.CreateToken(user))
            .Returns("JWT_TOKEN_123");

        // Act
        var result = await _sut.Login(dto, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeAssignableTo<AuthResponseDto>().Subject;

        response.Token.Should().Be("JWT_TOKEN_123");
        response.Email.Should().Be("dima@test.com");
        response.Name.Should().Be("Dima");
        response.Role.Should().Be("Admin");
    }

    // ------------------------------------------------------------
    // STATUS
    // ------------------------------------------------------------
    [Fact]
    public void GetStatus_WhenAuthenticated_ShouldReturnTrue()
    {
        // Arrange
        SetAuthenticatedUser();

        // Act
        var result = _sut.GetStatus();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;

        ok.Value.Should().BeEquivalentTo(new
        {
            IsAuthenticated = true
        });
    }

    [Fact]
    public void GetStatus_WhenAnonymous_ShouldReturnFalse()
    {
        // Arrange
        SetAnonymousUser();

        // Act
        var result = _sut.GetStatus();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;

        ok.Value.Should().BeEquivalentTo(new
        {
            IsAuthenticated = false
        });
    }
}
