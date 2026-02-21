using Auth.API.Models;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

/// <summary>
/// Authentication controller for login operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>API status message</returns>
    [HttpGet]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "API is running successfully",
            timestamp = DateTime.UtcNow,
            endpoints = new
            {
                login = "POST /api/auth/login",
                userInfo = "GET /api/secure/user (requires authentication)",
                adminInfo = "GET /api/secure/admin (requires Admin role)"
            }
        });
    }

    /// <summary>
    /// Login endpoint to generate JWT token
    /// </summary>
    /// <param name="request">Login request with username and password</param>
    /// <returns>Token response with JWT token if credentials are valid</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Login attempt with null request body");
            return BadRequest(new { message = "Request body is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login attempt with missing username or password");
            return BadRequest(new { message = "Username and password are required" });
        }

        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            _logger.LogWarning($"Failed login attempt for user: {request.Username}");
            return Unauthorized(new { message = "Invalid username or password" });
        }

        _logger.LogInformation($"Successful login for user: {request.Username}");
        return Ok(result);
    }
}
