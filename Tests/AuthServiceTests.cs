using Auth.API.Models;
using Auth.API.Services;
using Moq;
using Xunit;

namespace Auth.API.Tests;

/// <summary>
/// Unit tests for AuthService
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserStore> _mockUserStore;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserStore = new Mock<IUserStore>();
        _mockTokenService = new Mock<ITokenService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_mockUserStore.Object, _mockTokenService.Object, _mockLogger.Object);
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenResponse()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";
        var user = new User
        {
            Id = 1,
            Username = username,
            Password = password,
            Email = "test@example.com",
            Roles = new List<string> { "User" }
        };

        var loginRequest = new LoginRequest { Username = username, Password = password };
        var token = "jwt-token-string";

        _mockUserStore.Setup(x => x.GetUserByUsernameAsync(username))
            .ReturnsAsync(user);
        _mockTokenService.Setup(x => x.GenerateToken(user))
            .Returns(token);
        _mockTokenService.Setup(x => x.GetTokenExpirationSeconds())
            .Returns(3600);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result.Token);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Equal("Login successful", result.Message);
        // GetUserByUsernameAsync is called twice - once in LoginAsync and once in VerifyPasswordAsync
        _mockUserStore.Verify(x => x.GetUserByUsernameAsync(username), Times.Exactly(2));
    }

    [Fact]
    public async Task LoginAsync_WithNullRequest_ReturnsNull()
    {
        // Act
        var result = await _authService.LoginAsync(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "nonexistent", Password = "password" };
        _mockUserStore.Setup(x => x.GetUserByUsernameAsync("nonexistent"))
            .ReturnsAsync((User)null);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var username = "testuser";
        var user = new User
        {
            Id = 1,
            Username = username,
            Password = "correctpassword",
            Email = "test@example.com",
            Roles = new List<string> { "User" }
        };

        var loginRequest = new LoginRequest { Username = username, Password = "wrongpassword" };

        _mockUserStore.Setup(x => x.GetUserByUsernameAsync(username))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithExceptionInTokenGeneration_ReturnsNull()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";
        var user = new User
        {
            Id = 1,
            Username = username,
            Password = password,
            Email = "test@example.com",
            Roles = new List<string> { "User" }
        };

        var loginRequest = new LoginRequest { Username = username, Password = password };

        _mockUserStore.Setup(x => x.GetUserByUsernameAsync(username))
            .ReturnsAsync(user);
        _mockTokenService.Setup(x => x.GenerateToken(user))
            .Throws(new Exception("Token generation error"));

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region VerifyPasswordAsync Tests

    [Fact]
    public async Task VerifyPasswordAsync_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";
        var user = new User
        {
            Id = 1,
            Username = username,
            Password = password,
            Email = "test@example.com",
            Roles = new List<string> { "User" }
        };

        _mockUserStore.Setup(x => x.GetUserByUsernameAsync(username))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.VerifyPasswordAsync(username, password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var username = "testuser";
        var user = new User
        {
            Id = 1,
            Username = username,
            Password = "correctpassword",
            Email = "test@example.com",
            Roles = new List<string> { "User" }
        };

        _mockUserStore.Setup(x => x.GetUserByUsernameAsync(username))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.VerifyPasswordAsync(username, "wrongpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        _mockUserStore.Setup(x => x.GetUserByUsernameAsync("nonexistent"))
            .ReturnsAsync((User)null);

        // Act
        var result = await _authService.VerifyPasswordAsync("nonexistent", "password");

        // Assert
        Assert.False(result);
    }

    #endregion
}
