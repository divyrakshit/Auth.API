using Auth.API.Models;
using Auth.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Auth.API.Tests;

/// <summary>
/// Unit tests for TokenService
/// </summary>
public class TokenServiceTests
{
    private readonly IConfigurationRoot _configuration;
    private readonly Mock<ILogger<TokenService>> _mockLogger;

    public TokenServiceTests()
    {
        var configDictionary = new Dictionary<string, string>
        {
            ["Jwt:SecretKey"] = "this-is-a-very-long-secret-key-for-jwt-that-is-at-least-32-characters",
            ["Jwt:Issuer"] = "AuthAPI",
            ["Jwt:Audience"] = "AuthAPIUsers",
            ["Jwt:ExpirationMinutes"] = "60"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDictionary)
            .Build();

        _mockLogger = new Mock<ILogger<TokenService>>();
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidToken()
    {
        // Arrange
        var tokenService = new TokenService(_configuration, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Roles = new List<string> { "User", "Admin" }
        };

        // Act
        var token = tokenService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Validate token structure
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.NotNull(jwtToken);
        Assert.Equal("AuthAPI", jwtToken.Issuer);
        Assert.Contains("AuthAPIUsers", jwtToken.Audiences);
    }

    [Fact]
    public void GenerateToken_IncludesCorrectClaims()
    {
        // Arrange
        var tokenService = new TokenService(_configuration, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Roles = new List<string> { "User", "Admin" }
        };

        // Act
        var token = tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        var usernameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        var emailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
        var roleClaims = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).ToList();

        Assert.NotNull(userIdClaim);
        Assert.Equal("1", userIdClaim.Value);
        Assert.NotNull(usernameClaim);
        Assert.Equal("testuser", usernameClaim.Value);
        Assert.NotNull(emailClaim);
        Assert.Equal("test@example.com", emailClaim.Value);
        Assert.Equal(2, roleClaims.Count);
        Assert.Contains("User", roleClaims.Select(x => x.Value));
        Assert.Contains("Admin", roleClaims.Select(x => x.Value));
    }

    [Fact]
    public void GenerateToken_TokenHasCorrectExpiration()
    {
        // Arrange
        var tokenService = new TokenService(_configuration, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Roles = new List<string> { "User" }
        };

        var timeBefore = DateTime.UtcNow;

        // Act
        var token = tokenService.GenerateToken(user);

        var timeAfter = DateTime.UtcNow;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Token should expire in approximately 60 minutes
        var expectedExpiration = timeBefore.AddMinutes(60);
        var expiryDifference = Math.Abs((jwtToken.ValidTo - expectedExpiration).TotalSeconds);

        Assert.True(expiryDifference < 5, "Token expiration should be within 5 seconds of expected time");
    }

    [Fact]
    public void GenerateToken_WithUserWithNoRoles_ReturnsToken()
    {
        // Arrange
        var tokenService = new TokenService(_configuration, _mockLogger.Object);
        var user = new User
        {
            Id = 2,
            Username = "noroleuser",
            Email = "norole@example.com",
            Roles = new List<string>()
        };

        // Act
        var token = tokenService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaims = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).ToList();

        Assert.Empty(roleClaims);
    }

    [Fact]
    public void GetTokenExpirationSeconds_ReturnsCorrectValue()
    {
        // Arrange
        var tokenService = new TokenService(_configuration, _mockLogger.Object);

        // Act
        var expirationSeconds = tokenService.GetTokenExpirationSeconds();

        // Assert
        Assert.Equal(3600, expirationSeconds); // 60 minutes * 60 seconds
    }

    [Fact]
    public void Constructor_WithInvalidSecretKey_ThrowsException()
    {
        // Arrange
        var invalidConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:SecretKey"] = "short",
                ["Jwt:Issuer"] = "AuthAPI",
                ["Jwt:Audience"] = "AuthAPIUsers",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new TokenService(invalidConfig, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithMissingSecretKey_ThrowsException()
    {
        // Arrange
        var invalidConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Issuer"] = "AuthAPI",
                ["Jwt:Audience"] = "AuthAPIUsers",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new TokenService(invalidConfig, _mockLogger.Object));
    }
}
