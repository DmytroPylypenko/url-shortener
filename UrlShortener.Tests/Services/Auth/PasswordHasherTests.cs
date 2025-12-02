using FluentAssertions;
using UrlShortener.Web.Services.Auth;   

namespace UrlShortener.Tests.Services.Auth;

public class PasswordHasherTests
{
    private readonly PbkdfPasswordHasher _sut;

    public PasswordHasherTests()
    {
        _sut = new PbkdfPasswordHasher();
    }

    [Fact]
    public void Hash_WhenPasswordIsProvided_ShouldReturnNonEmptyHash()
    {
        // Arrange
        var password = "mySecurePassword123";

        // Act
        var hashedPassword = _sut.Hash(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().Contain(":"); // Checks that our salt:hash format is correct
    }

    [Fact]
    public void Hash_WhenSamePasswordIsHashedTwice_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "SamePassword";

        // Act
        var hash1 = _sut.Hash(password);
        var hash2 = _sut.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }
    
    [Fact]
    public void Verify_WhenPasswordIsCorrect_ShouldReturnTrue()
    {
        // Arrange
        var password = "mySecurePassword123";
        var hashedPassword = _sut.Hash(password);

        // Act
        var isVerified = _sut.Verify(hashedPassword, password);

        // Assert
        isVerified.Should().BeTrue();
    }

    [Fact]
    public void Verify_WhenPasswordIsIncorrect_ShouldReturnFalse()
    {
        // Arrange
        var password = "mySecurePassword123";
        var wrongPassword = "wrongPassword";
        var hashedPassword = _sut.Hash(password);

        // Act
        var isVerified = _sut.Verify(hashedPassword, wrongPassword);

        // Assert
        isVerified.Should().BeFalse();
    }

    [Fact]
    public void Hash_WhenPasswordIsEmpty_ShouldStillProduceValidHash()
    {
        // Arrange
        var password = string.Empty;

        // Act
        var hash = _sut.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().Contain(":");
    }
    
    [Fact]
    public void Verify_WhenPasswordIsEmpty_ShouldReturnTrueIfMatchesHash()
    {
        // Arrange
        var password = string.Empty;
        var hash = _sut.Hash(password);

        // Act
        var result = _sut.Verify(hash, password);

        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void Hash_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var password = "PerformanceTest123";

        // Act
        var watch = System.Diagnostics.Stopwatch.StartNew();
        _sut.Hash(password);
        watch.Stop();

        // Assert
        watch.ElapsedMilliseconds.Should().BeLessThan(500, "hashing should be efficient");
    }
}