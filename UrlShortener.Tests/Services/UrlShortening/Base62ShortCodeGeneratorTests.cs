using FluentAssertions;
using UrlShortener.Application.Services.UrlShortening;

namespace UrlShortener.Tests.Services.UrlShortening;

public class Base62ShortCodeGeneratorTests
{
    private readonly Base62ShortCodeGenerator _sut = new();

    // Base62
    private const string AllowedChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // ---------------------------------------------------------------
    // GENERATE 
    // ---------------------------------------------------------------
    [Fact]
    public void Generate_ShouldReturnNonEmptyString()
    {
        // Act
        var code = _sut.Generate();

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generate_ShouldContainOnlyBase62Characters()
    {
        // Act
        var code = _sut.Generate();

        // Assert
        code.ToCharArray().Should().OnlyContain(c => AllowedChars.Contains(c));
    }

    [Fact]
    public void Generate_ShouldReturnDifferentValues()
    {
        // Act
        var code1 = _sut.Generate();
        var code2 = _sut.Generate();

        // Assert
        code1.Should().NotBe(code2);
    }

    [Fact]
    public void Generate_ShouldHaveLengthBetween1And11()
    {
        // Act
        var code = _sut.Generate();

        // Assert
        code.Length.Should().BeInRange(1, 11);
    }

    // ---------------------------------------------------------------
    // INDIRECT TESTS FOR EncodeBase62
    // ---------------------------------------------------------------
    [Fact]
    public void EncodeBase62_ShouldReturnZero_ForValueZero()
    {
        // Act 
        var encoded = CallEncodeBase62(0);

        // Assert
        encoded.Should().Be("0");
    }

    [Fact]
    public void EncodeBase62_ShouldEncodeKnownValuesCorrectly()
    {
        CallEncodeBase62(1).Should().Be("1");
        CallEncodeBase62(10).Should().Be("a");  
        CallEncodeBase62(61).Should().Be("Z");  
        CallEncodeBase62(62).Should().Be("10"); 
        CallEncodeBase62(12345).Should().Be("3d7"); 
    }

    // ---------------------------------------------------------------
    // HELPER: Call the private EncodeBase62() via reflection
    // ---------------------------------------------------------------
    private static string CallEncodeBase62(ulong value)
    {
        var method = typeof(Base62ShortCodeGenerator)
            .GetMethod("EncodeBase62", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        return (string)method!.Invoke(null, [value])!;
    }
}
