using FluentAssertions;
using Moq;
using UrlShortener.Web.Domain.Entities;
using UrlShortener.Web.Domain.Interfaces;
using UrlShortener.Web.Services.UrlShortening;

namespace UrlShortener.Tests.Services.UrlShortening;

public class UrlShorteningServiceTests
{
    private readonly Mock<IUrlRecordRepository> _mockRepo;
    private readonly Mock<IShortCodeGenerator> _mockGenerator;
    private readonly UrlShorteningService _sut;

    public UrlShorteningServiceTests()
    {
        _mockRepo = new Mock<IUrlRecordRepository>();
        _mockGenerator = new Mock<IShortCodeGenerator>();

        _sut = new UrlShorteningService(
            _mockRepo.Object,
            _mockGenerator.Object);
    }

    // ------------------------------------------------------------
    // CreateAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateAsync_WhenUrlInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidUrl = "not_a_url";

        // Act
        Func<Task> act = () => _sut.CreateAsync(invalidUrl, 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid URL format*");
    }

    [Fact]
    public async Task CreateAsync_WhenUrlAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockRepo.Setup(r => r.UrlExistsAsync("https://google.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        // Act
        Func<Task> act = () => _sut.CreateAsync("https://google.com", 1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateAsync_ShouldRegenerateShortCodeUntilUnique()
    {
        // Arrange
        var url = "https://example.com";

        _mockRepo.Setup(r => r.UrlExistsAsync(url, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
        
        _mockGenerator.SetupSequence(g => g.Generate())
                      .Returns("dup")      
                      .Returns("unique"); 

        _mockRepo.SetupSequence(r => r.ShortCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true)   
                 .ReturnsAsync(false); 

        UrlRecord? capturedRecord = null;

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<UrlRecord>(), It.IsAny<CancellationToken>()))
                 .Callback((UrlRecord r, CancellationToken _) => capturedRecord = r)
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(url, 99);

        // Assert
        result.ShortCode.Should().Be("unique");

        capturedRecord.Should().NotBeNull();
        capturedRecord!.ShortCode.Should().Be("unique");
        capturedRecord.OriginalUrl.Should().Be(url);
        capturedRecord.CreatedByUserId.Should().Be(99);

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<UrlRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRecordSuccessfully()
    {
        // Arrange
        var url = "https://dotnet.microsoft.com";

        _mockRepo.Setup(r => r.UrlExistsAsync(url, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        _mockGenerator.Setup(g => g.Generate()).Returns("abc123");
        _mockRepo.Setup(r => r.ShortCodeExistsAsync("abc123", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        UrlRecord? captured = null;

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<UrlRecord>(), It.IsAny<CancellationToken>()))
                 .Callback((UrlRecord r, CancellationToken _) => captured = r)
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(url, 42);

        // Assert
        result.ShortCode.Should().Be("abc123");
        result.OriginalUrl.Should().Be(url);
        result.CreatedByUserId.Should().Be(42);
        result.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        captured.Should().NotBeNull();
        captured!.OriginalUrl.Should().Be(url);

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<UrlRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
