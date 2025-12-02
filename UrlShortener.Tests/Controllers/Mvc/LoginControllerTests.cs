using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using UrlShortener.Web.Configuration;
using UrlShortener.Web.Controllers.Mvc;
using UrlShortener.Web.Models.Auth;

namespace UrlShortener.Tests.Controllers.Mvc;

public class LoginControllerTests
{
    private readonly Mock<IHttpClientFactory> _mockClientFactory;
    private readonly ApiSettings _apiSettings;

    private readonly LoginController _sut;

    public LoginControllerTests()
    {
        _mockClientFactory = new Mock<IHttpClientFactory>();

        _apiSettings = new ApiSettings
        {
            BaseUrl = "https://api.example.com/"
        };

        _sut = new LoginController(
            _mockClientFactory.Object,
            Options.Create(_apiSettings)
        );

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // -------------------------------------------------------------------
    // Helper: create HttpClient with fake handler
    // -------------------------------------------------------------------
    private HttpClient CreateFakeHttpClient(HttpStatusCode code, object? responseJson = null)
    {
        var handler = new FakeHttpMessageHandler(code, responseJson);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri(_apiSettings.BaseUrl)
        };
    }

    // -------------------------------------------------------------------
    // GET /Login
    // -------------------------------------------------------------------
    [Fact]
    public void Index_Get_ShouldReturnViewWithEmptyModel()
    {
        // Act
        var result = _sut.Index();

        // Assert
        var view = result.Should().BeOfType<ViewResult>().Subject;

        view.Model.Should().BeAssignableTo<LoginViewModel>();
    }

    // -------------------------------------------------------------------
    // POST /Login
    // -------------------------------------------------------------------
    [Fact]
    public async Task Index_Post_WhenModelStateInvalid_ShouldReturnView()
    {
        // Arrange
        _sut.ModelState.AddModelError("Email", "Required");
        var model = new LoginViewModel();

        // Act
        var result = await _sut.Index(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Index_Post_WhenApiReturnsFailure_ShouldReturnViewWithError()
    {
        // Arrange
        var model = new LoginViewModel 
        { 
            Email = "a@a.com", 
            Password = "123" 
        };

        var client = CreateFakeHttpClient(HttpStatusCode.Unauthorized);
        _mockClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        // Act
        var result = await _sut.Index(model);

        // Assert
        var view = result.Should().BeOfType<ViewResult>().Subject;
        var returnedModel = view.Model.Should().BeAssignableTo<LoginViewModel>().Subject;

        returnedModel.ErrorMessage.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Index_Post_WhenLoginSuccessful_ShouldSetCookieAndRedirect()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Email = "test@test.com",
            Password = "pass123"
        };

        var apiResponse = new Dictionary<string, string>
        {
            ["token"] = "JWT_TOKEN_ABC"
        };

        var client = CreateFakeHttpClient(HttpStatusCode.OK, apiResponse);
        _mockClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        // Act
        var result = await _sut.Index(model);

        // Assert
        // 1. Cookie is set
        var setCookie = _sut.HttpContext.Response.Headers["Set-Cookie"].ToString();

        setCookie.Should().Contain("auth_token=");
        setCookie.Should().Contain("JWT_TOKEN_ABC");

        // 2. Redirected
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        redirect.ControllerName.Should().Be("Home");
    }

    // -------------------------------------------------------------------
    // GET /Logout
    // -------------------------------------------------------------------
    [Fact]
    public void Logout_ShouldDeleteCookieAndRedirect()
    {
        // Arrange
        _sut.HttpContext.Response.Cookies.Append("auth_token", "XYZ");

        // Act
        var result = _sut.Logout();

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
    }
}

// -------------------------------------------------------------------
// FAKE HTTP HANDLER FOR MOCKING HTTPCLIENT
// -------------------------------------------------------------------
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly object? _jsonObj;

    public FakeHttpMessageHandler(HttpStatusCode status, object? jsonObj = null)
    {
        _status = status;
        _jsonObj = jsonObj;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_status);

        if (_jsonObj != null)
        {
            response.Content = JsonContent.Create(_jsonObj);
        }

        return Task.FromResult(response);
    }
}
