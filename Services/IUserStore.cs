using Auth.API.Models;

namespace Auth.API.Services;

/// <summary>
/// Interface for user data persistence
/// </summary>
public interface IUserStore
{
    /// <summary>
    /// Get user by username
    /// </summary>
    Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// Get all users
    /// </summary>
    Task<List<User>> GetAllUsersAsync();

    /// <summary>
    /// Add a user
    /// </summary>
    Task<User> AddUserAsync(User user);
}
