using Auth.API.Models;
using Auth.API.Services;
using Moq;
using Xunit;

namespace Auth.API.Tests;

/// <summary>
/// Unit tests for InMemoryUserStore
/// </summary>
public class InMemoryUserStoreTests
{
    private readonly Mock<ILogger<InMemoryUserStore>> _mockLogger;
    private readonly InMemoryUserStore _userStore;

    public InMemoryUserStoreTests()
    {
        _mockLogger = new Mock<ILogger<InMemoryUserStore>>();
        _userStore = new InMemoryUserStore(_mockLogger.Object);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WithValidUsername_ReturnsUser()
    {
        // Act
        var result = await _userStore.GetUserByUsernameAsync("admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin", result.Username);
        Assert.Equal("admin123", result.Password);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WithInvalidUsername_ReturnsNull()
    {
        // Act
        var result = await _userStore.GetUserByUsernameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Act
        var result = await _userStore.GetUserByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _userStore.GetUserByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllDefaultUsers()
    {
        // Act
        var result = await _userStore.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, u => u.Username == "admin");
        Assert.Contains(result, u => u.Username == "user");
        Assert.Contains(result, u => u.Username == "moderator");
    }

    [Fact]
    public async Task AddUserAsync_AddsNewUser()
    {
        // Arrange
        var newUser = new User
        {
            Username = "newuser",
            Password = "newpass",
            Email = "new@example.com",
            Roles = new List<string> { "User" }
        };

        // Act
        var result = await _userStore.AddUserAsync(newUser);
        var addedUser = await _userStore.GetUserByUsernameAsync("newuser");

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.NotNull(addedUser);
        Assert.Equal("newuser", addedUser.Username);
    }

    [Fact]
    public async Task DefaultUsers_HaveCorrectRoles()
    {
        // Act
        var adminUser = await _userStore.GetUserByUsernameAsync("admin");
        var regularUser = await _userStore.GetUserByUsernameAsync("user");
        var moderatorUser = await _userStore.GetUserByUsernameAsync("moderator");

        // Assert
        Assert.Contains("Admin", adminUser.Roles);
        Assert.Contains("User", adminUser.Roles);

        Assert.Single(regularUser.Roles);
        Assert.Contains("User", regularUser.Roles);

        Assert.Contains("Moderator", moderatorUser.Roles);
        Assert.Contains("User", moderatorUser.Roles);
    }

    [Fact]
    public async Task AddUserAsync_IncrementIdCorrectly()
    {
        // Arrange
        var allUsersBefore = await _userStore.GetAllUsersAsync();
        var lastId = allUsersBefore.Max(u => u.Id);

        var newUser = new User
        {
            Username = "another",
            Password = "pass",
            Email = "another@example.com",
            Roles = new List<string> { "User" }
        };

        // Act
        var result = await _userStore.AddUserAsync(newUser);

        // Assert
        Assert.Equal(lastId + 1, result.Id);
    }
}
