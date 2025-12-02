using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Web.Controllers.Mvc;

namespace UrlShortener.Tests.Controllers.Mvc;

public class AboutControllerTests
{
    private readonly Mock<IAboutContentRepository> _mockRepo;
    private readonly AboutController _sut;

    public AboutControllerTests()
    {
        _mockRepo = new Mock<IAboutContentRepository>();
        _sut = new AboutController(_mockRepo.Object);
    }

    // --------------------------------------------------------------------
    // Helpers
    // --------------------------------------------------------------------
    private void SetUser(bool isAdmin = false, int? userId = null)
    {
        var claims = new List<Claim>();

        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetAnonymous()
    {
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // --------------------------------------------------------------------
    // GET INDEX
    // --------------------------------------------------------------------
    [Fact]
    public async Task Index_Get_WhenUserIsAdmin_ShouldSetIsAdminTrue()
    {
        // Arrange
        SetUser(isAdmin: true);

        var content = new AboutContent { Content = "Test content" };

        _mockRepo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _sut.Index(CancellationToken.None);

        // Assert
        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().Be(content);

        _sut.ViewData["IsAdmin"].Should().Be(true);
    }

    [Fact]
    public async Task Index_Get_WhenUserIsNotAdmin_ShouldSetIsAdminFalse()
    {
        // Arrange
        SetUser(isAdmin: false);

        var content = new AboutContent { Content = "Test" };
        _mockRepo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _sut.Index(CancellationToken.None);

        // Assert
        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().Be(content);

        _sut.ViewData["IsAdmin"].Should().Be(false);
    }

    [Fact]
    public async Task Index_Get_WhenAnonymous_ShouldSetIsAdminFalse()
    {
        // Arrange
        SetAnonymous();

        var content = new AboutContent { Content = "Hello" };
        _mockRepo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _sut.Index(CancellationToken.None);

        // Assert
        _ = result.Should().BeOfType<ViewResult>().Subject;

        _sut.ViewData["IsAdmin"].Should().Be(false);
    }

    // --------------------------------------------------------------------
    // POST INDEX (UPDATE)
    // --------------------------------------------------------------------
    [Fact]
    public async Task Index_Post_WhenModelStateInvalid_ShouldReturnViewWithExistingContent()
    {
        // Arrange
        SetUser(isAdmin: true);
        _sut.ModelState.AddModelError("Content", "Required");

        var existing = new AboutContent { Content = "Existing" };

        _mockRepo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var posted = new AboutContent { Content = "" }; // invalid

        // Act
        var result = await _sut.Index(posted, CancellationToken.None);

        // Assert
        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().Be(existing);
        _sut.ViewData["IsAdmin"].Should().Be(true);
    }

    [Fact]
    public async Task Index_Post_WhenContentNotFound_ShouldReturnNotFound()
    {
        // Arrange
        SetUser(isAdmin: true);

        _mockRepo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((AboutContent?)null);

        var posted = new AboutContent { Content = "New text" };

        // Act
        var result = await _sut.Index(posted, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Index_Post_WhenValid_ShouldUpdateContentAndRedirect()
    {
        // Arrange
        SetUser(isAdmin: true, userId: 10);

        var contentRecord = new AboutContent
        {
            Id = 1,
            Content = "Old content",
            UpdatedByUserId = null
        };

        _mockRepo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentRecord);

        var posted = new AboutContent { Content = "Updated content" };

        // Act
        var result = await _sut.Index(posted, CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.UpdateAsync(contentRecord), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        contentRecord.Content.Should().Be("Updated content");
        contentRecord.UpdatedByUserId.Should().Be(10);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(AboutController.Index));
    }
}
