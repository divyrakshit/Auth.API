using Auth.API.Models;

namespace Auth.API.Services;

/// <summary>
/// Interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login a user with username and password
    /// </summary>
    Task<TokenResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// Verify password for a user
    /// </summary>
    Task<bool> VerifyPasswordAsync(string username, string password);
}
