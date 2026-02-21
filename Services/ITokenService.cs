using Auth.API.Models;

namespace Auth.API.Services;

/// <summary>
/// Interface for token generation and validation
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate a JWT token for the given user
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Get token expiration time in seconds
    /// </summary>
    int GetTokenExpirationSeconds();
}
