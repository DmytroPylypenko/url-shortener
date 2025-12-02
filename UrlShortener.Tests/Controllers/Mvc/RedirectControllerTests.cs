using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UrlShortener.Web.Controllers.Mvc;
using UrlShortener.Web.Domain.Entities;
using UrlShortener.Web.Domain.Interfaces;

namespace UrlShortener.Tests.Controllers.Mvc;

public class RedirectControllerTests
{
    private readonly Mock<IUrlRecordRepository> _mockRepo;
    private readonly RedirectController _sut;

    public RedirectControllerTests()
    {
        _mockRepo = new Mock<IUrlRecordRepository>();
        _sut = new RedirectController(_mockRepo.Object);
    }
    
    [Fact]
    public async Task RedirectToOriginal_WhenShortCodeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByShortCodeAsync("xyz", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((UrlRecord?)null);

        // Act
        var result = await _sut.RedirectToOriginal("xyz", CancellationToken.None);

        // Assert
        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().Be("Short URL not found.");
    }

    [Fact]
    public async Task RedirectToOriginal_WhenFound_ShouldRedirectToOriginalUrl()
    {
        // Arrange
        var record = new UrlRecord
        {
            Id = 1,
            OriginalUrl = "https://google.com",
            ShortCode = "abc",
            VisitCount = 5,
            LastAccessedAtUtc = DateTime.UtcNow.AddHours(-1)
        };

        _mockRepo.Setup(r => r.GetByShortCodeAsync("abc", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(record);

        // Act
        var result = await _sut.RedirectToOriginal("abc", CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("https://google.com");
    }
    
    [Fact]
    public async Task RedirectToOriginal_ShouldIncrementVisitCount()
    {
        // Arrange
        var record = new UrlRecord
        {
            VisitCount = 0,
            ShortCode = "test",
            OriginalUrl = "https://example.com"
        };

        _mockRepo.Setup(r => r.GetByShortCodeAsync("test", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(record);

        // Act
        await _sut.RedirectToOriginal("test", CancellationToken.None);

        // Assert
        record.VisitCount.Should().Be(1);
    }

    [Fact]
    public async Task RedirectToOriginal_ShouldUpdateLastAccessedTimestamp()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddDays(-1);

        var record = new UrlRecord
        {
            LastAccessedAtUtc = oldDate,
            OriginalUrl = "https://example.com",
            ShortCode = "code"
        };

        _mockRepo.Setup(r => r.GetByShortCodeAsync("code", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(record);

        // Act
        await _sut.RedirectToOriginal("code", CancellationToken.None);

        // Assert
        record.LastAccessedAtUtc.Should().BeAfter(oldDate);
    }

    [Fact]
    public async Task RedirectToOriginal_WhenFound_ShouldCallSaveChanges()
    {
        // Arrange
        var record = new UrlRecord
        {
            ShortCode = "z9",
            OriginalUrl = "https://microsoft.com"
        };

        _mockRepo.Setup(r => r.GetByShortCodeAsync("z9", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(record);

        // Act
        await _sut.RedirectToOriginal("z9", CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
