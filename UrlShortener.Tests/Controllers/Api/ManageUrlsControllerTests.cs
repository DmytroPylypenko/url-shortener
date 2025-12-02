using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Web.Controllers.Api;
using UrlShortener.Web.Models.Url;

namespace UrlShortener.Tests.Controllers.Api;

public class ManageUrlsControllerTests
{
    private readonly Mock<IUrlRecordRepository> _mockRepo;
    private readonly Mock<IUrlShorteningService> _mockShortening;
    private readonly ManageUrlsController _sut;

    public ManageUrlsControllerTests()
    {
        _mockRepo = new Mock<IUrlRecordRepository>();
        _mockShortening = new Mock<IUrlShorteningService>();

        _sut = new ManageUrlsController(_mockRepo.Object, _mockShortening.Object);
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------
    private void SetUser(int userId, string role = "User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ------------------------------------------------------------
    // CREATE
    // ------------------------------------------------------------
    [Fact]
    public async Task Create_WhenModelStateInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        SetUser(1);
        _sut.ModelState.AddModelError("OriginalUrl", "Required");
        var dto = new CreateShortUrlRequestDto();

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange — user has NO claims
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var dto = new CreateShortUrlRequestDto { OriginalUrl = "https://google.com" };

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValidRequest_ShouldReturnOkWithResponseDto()
    {
        // Arrange
        SetUser(5);

        var dto = new CreateShortUrlRequestDto
        {
            OriginalUrl = "https://example.com"
        };

        var createdRecord = new UrlRecord
        {
            Id = 10,
            CreatedByUserId = 5,
            OriginalUrl = "https://example.com",
            ShortCode = "abc123"
        };

        _mockShortening
            .Setup(s => s.CreateAsync(dto.OriginalUrl, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRecord);

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeAssignableTo<CreateShortUrlResponseDto>().Subject;

        response.Id.Should().Be(10);
        response.OriginalUrl.Should().Be("https://example.com");
        response.ShortCode.Should().Be("abc123");
    }

    // ------------------------------------------------------------
    // DELETE
    // ------------------------------------------------------------
    [Fact]
    public async Task Delete_WhenRecordNotFound_ShouldReturnNotFound()
    {
        // Arrange
        SetUser(1);
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UrlRecord?)null);

        // Act
        var result = await _sut.Delete(99, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_WhenUnauthorizedMissingUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var record = new UrlRecord { Id = 15, CreatedByUserId = 5 };

        _mockRepo.Setup(r => r.GetByIdAsync(15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext 
            { 
                User = new ClaimsPrincipal(new ClaimsIdentity()) 
            }
        };
        
        // Act — no user claims set
        var result = await _sut.Delete(15, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Delete_WhenUserNotOwner_ShouldReturnForbid()
    {
        // Arrange
        SetUser(userId: 2, role: "User");

        var record = new UrlRecord { Id = 1, CreatedByUserId = 5 };

        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _sut.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Delete_WhenAdmin_ShouldDeleteAnyRecord()
    {
        // Arrange
        SetUser(userId: 777, role: "Admin");

        var record = new UrlRecord { Id = 50, CreatedByUserId = 3 };

        _mockRepo.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _sut.Delete(50, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mockRepo.Verify(r => r.DeleteAsync(record, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenUserIsOwner_ShouldDeleteRecord()
    {
        // Arrange
        SetUser(userId: 5, role: "User");

        var record = new UrlRecord { Id = 1, CreatedByUserId = 5 };

        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _sut.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mockRepo.Verify(r => r.DeleteAsync(record, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
