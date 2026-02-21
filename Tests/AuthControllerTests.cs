using Auth.API.Controllers;
using Auth.API.Models;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Auth.API.Tests;

/// <summary>
/// Unit tests for AuthController
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "admin", Password = "admin123" };
        var tokenResponse = new TokenResponse
        {
            Token = "jwt-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Message = "Login successful"
        };

        _mockAuthService.Setup(x => x.LoginAsync(loginRequest))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedToken = Assert.IsType<TokenResponse>(okResult.Value);
        Assert.Equal("jwt-token", returnedToken.Token);
        _mockAuthService.Verify(x => x.LoginAsync(loginRequest), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "admin", Password = "wrongpassword" };
        _mockAuthService.Setup(x => x.LoginAsync(loginRequest))
            .ReturnsAsync((TokenResponse)null);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Login(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingUsername_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "", Password = "password" };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "admin", Password = "" };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithNullUsername_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = null, Password = "password" };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithNullPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "admin", Password = null };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}
