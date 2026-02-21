using Auth.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Auth.API.Tests;

/// <summary>
/// Unit tests for SecureController authorization
/// </summary>
public class SecureControllerTests
{
    private readonly Mock<ILogger<SecureController>> _mockLogger;
    private readonly SecureController _controller;

    public SecureControllerTests()
    {
        _mockLogger = new Mock<ILogger<SecureController>>();
        _controller = new SecureController(_mockLogger.Object);
    }

    #region GetUser Tests

    [Fact]
    public void GetUser_WithAuthenticatedUser_ReturnsOkWithUserInfo()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "User")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.GetUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void GetUser_ReturnsCorrectUserClaims()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "john_doe"),
            new Claim(ClaimTypes.Email, "john@example.com"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Moderator")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.GetUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var value = okResult.Value as dynamic;
        Assert.NotNull(value);
        Assert.Equal("123", (string)value.id);
        Assert.Equal("john_doe", (string)value.username);
        Assert.Equal("john@example.com", (string)value.email);
    }

    #endregion

    #region GetAdmin Tests

    [Fact]
    public void GetAdmin_WithAdminUser_ReturnsOkWithAdminInfo()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.GetAdmin();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void GetAdmin_WithAdminUser_ReturnsCorrectAdminClaims()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "admin_user"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.GetAdmin();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var value = okResult.Value as dynamic;
        Assert.NotNull(value);
        Assert.Equal("1", (string)value.id);
        Assert.Equal("admin_user", (string)value.username);
    }

    [Fact]
    public void GetAdmin_ReturnsAdminRolesInResponse()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Moderator")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.GetAdmin();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value as dynamic;
        Assert.NotNull(value);
        var roles = value.roles as List<string>;
        Assert.NotNull(roles);
        Assert.Equal(3, roles.Count);
        Assert.Contains("Admin", roles);
        Assert.Contains("User", roles);
        Assert.Contains("Moderator", roles);
    }

    [Fact]
    public void GetUser_WithMultipleRoles_ReturnsAllRoles()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Moderator"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = _controller.GetUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value as dynamic;
        var roles = value.roles as List<string>;
        Assert.NotNull(roles);
        Assert.Equal(3, roles.Count);
        Assert.Contains("User", roles);
        Assert.Contains("Moderator", roles);
        Assert.Contains("Admin", roles);
    }

    #endregion
}
