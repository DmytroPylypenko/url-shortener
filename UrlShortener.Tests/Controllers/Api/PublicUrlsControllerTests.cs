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

public class PublicUrlsControllerTests
{
    private readonly Mock<IUrlRecordRepository> _mockRepo;
    private readonly PublicUrlsController _sut;

    public PublicUrlsControllerTests()
    {
        _mockRepo = new Mock<IUrlRecordRepository>();
        _sut = new PublicUrlsController(_mockRepo.Object);
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------
    private void SetUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "1")],
            "TestAuth"));
        
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
    
    private void SetAnonymous()
    {
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ------------------------------------------------------------
    // GET ALL
    // ------------------------------------------------------------
    [Fact]
    public async Task GetAll_ShouldReturnListOfDtos()
    {
        // Arrange
        SetAnonymous(); // anonymous allowed

        var records = new List<UrlRecord>
        {
            new()
            {
                Id = 1,
                OriginalUrl = "https://google.com",
                ShortCode = "abc",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                VisitCount = 10,
                CreatedByUser = new User { Name = "John" }
            },
            new()
            {
                Id = 2,
                OriginalUrl = "https://youtube.com",
                ShortCode = "xyz",
                CreatedAtUtc = DateTime.UtcNow.AddHours(-5),
                VisitCount = 20,
                CreatedByUser = new User { Name = "Alice" }
            }
        };

        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = ok.Value.Should().BeAssignableTo<IEnumerable<UrlRecordListItemDto>>().Subject.ToList();

        list.Count.Should().Be(2);
        list[0].ShortCode.Should().Be("abc");
        list[0].CreatedBy.Should().Be("John");
        list[1].ShortCode.Should().Be("xyz");
        list[1].CreatedBy.Should().Be("Alice");

        _mockRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ------------------------------------------------------------
    // GET BY ID
    // ------------------------------------------------------------
    [Fact]
    public async Task GetById_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAnonymous(); 

        _mockRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UrlRecord?)null);
        
        // Act
        var result = await _sut.GetById(10, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetById_WhenRecordNotFound_ShouldReturnNotFound()
    {
        // Arrange
        SetUser();

        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UrlRecord?)null);

        // Act
        var result = await _sut.GetById(99, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_WhenRecordExists_ShouldReturnDto()
    {
        // Arrange
        SetUser();

        var record = new UrlRecord
        {
            Id = 5,
            OriginalUrl = "https://github.com",
            ShortCode = "git",
            CreatedAtUtc = DateTime.UtcNow,
            LastAccessedAtUtc = DateTime.UtcNow.AddMinutes(-30),
            VisitCount = 42,
            CreatedByUser = new User { Name = "Dima" }
        };

        _mockRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _sut.GetById(5, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeAssignableTo<UrlRecordDetailsDto>().Subject;

        dto.Id.Should().Be(5);
        dto.OriginalUrl.Should().Be("https://github.com");
        dto.ShortCode.Should().Be("git");
        dto.CreatedBy.Should().Be("Dima");
        dto.VisitCount.Should().Be(42);

        _mockRepo.Verify(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }
}