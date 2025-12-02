using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Web.Controllers.Mvc;

namespace UrlShortener.Tests.Controllers.Mvc;

public class InfoControllerTests
{
    private readonly Mock<IUrlRecordRepository> _mockRepo;
    private readonly InfoController _sut;

    public InfoControllerTests()
    {
        _mockRepo = new Mock<IUrlRecordRepository>();
        _sut = new InfoController(_mockRepo.Object);
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------
    private void SetAuthenticatedUser()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "1")], 
            "TestAuth"
        );

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
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

    // -------------------------------------------------------
    // TESTS
    // -------------------------------------------------------

    [Fact]
    public async Task Index_WhenIdIsZeroOrNegative_ShouldReturnBadRequest()
    {
        // Arrange
        SetAuthenticatedUser(); 

        // Act
        var result = await _sut.Index(0, CancellationToken.None);

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().Be("Invalid URL ID specified.");
    }

    [Fact]
    public async Task Index_WhenRecordNotFound_ShouldReturnNotFound()
    {
        // Arrange
        SetAuthenticatedUser();

        _mockRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UrlRecord?)null);

        // Act
        var result = await _sut.Index(5, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenRecordExists_ShouldReturnViewWithModel()
    {
        // Arrange
        SetAuthenticatedUser();

        var record = new UrlRecord
        {
            Id = 5,
            OriginalUrl = "https://example.com",
            ShortCode = "abc123",
            CreatedByUserId = 1
        };

        _mockRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _sut.Index(5, CancellationToken.None);

        // Assert
        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().Be(record);

        _mockRepo.Verify(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Index_WhenAnonymousUser_ShouldStillRequireAuthentication()
    {
        // Arrange
        SetAnonymousUser(); 
        _mockRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UrlRecord?)null);

        // Act
        var result = await _sut.Index(10, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
