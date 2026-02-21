using Auth.API.Models;

namespace Auth.API.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserStore _userStore;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserStore userStore, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _userStore = userStore;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            if (request == null)
            {
                _logger.LogWarning("LoginAsync called with null request");
                return null;
            }

            var user = await _userStore.GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning($"Login failed: User not found - {request.Username}");
                return null;
            }

            if (!await VerifyPasswordAsync(request.Username, request.Password))
            {
                _logger.LogWarning($"Login failed: Invalid password for user - {request.Username}");
                return null;
            }

            var token = _tokenService.GenerateToken(user);
            var expirationSeconds = _tokenService.GetTokenExpirationSeconds();

            _logger.LogInformation($"User logged in successfully: {request.Username}");
            return new TokenResponse
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = expirationSeconds,
                Message = "Login successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> VerifyPasswordAsync(string username, string password)
    {
        var user = await _userStore.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return false;
        }

        return user.Password == password; // In production, use secure password hashing
    }
}
