using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.API.Controllers;

/// <summary>
/// Secure controller with protected endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecureController : ControllerBase
{
    private readonly ILogger<SecureController> _logger;

    public SecureController(ILogger<SecureController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Authenticated endpoint accessible by any logged-in user
    /// </summary>
    /// <returns>User information from JWT claims</returns>
    [HttpGet("user")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation($"User endpoint accessed by: {username}");

        var userInfo = new
        {
            id = userId,
            username = username,
            email = email,
            roles = roles,
            message = "This is a secured endpoint accessible to all authenticated users"
        };

        return Ok(userInfo);
    }

    /// <summary>
    /// Admin-only endpoint that requires Admin role
    /// </summary>
    /// <returns>Admin information and access confirmation</returns>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAdmin()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation($"Admin endpoint accessed by: {username}");

        var adminInfo = new
        {
            id = userId,
            username = username,
            roles = roles,
            message = "This is an admin-only endpoint. Access granted because you have the Admin role."
        };

        return Ok(adminInfo);
    }

    /// <summary>
    /// Moderator-only endpoint that requires Moderator role
    /// </summary>
    /// <returns>Moderator information and access confirmation</returns>
    [HttpGet("moderator")]
    [Authorize(Roles = "Moderator")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetModerator()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation($"Moderator endpoint accessed by: {username}");

        var moderatorInfo = new
        {
            id = userId,
            username = username,
            roles = roles,
            message = "This is a moderator-only endpoint. Access granted because you have the Moderator role."
        };

        return Ok(moderatorInfo);
    }
}
