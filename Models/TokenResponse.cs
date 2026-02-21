namespace Auth.API.Models;

/// <summary>
/// JWT token response model
/// </summary>
public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string Message { get; set; } = string.Empty;
}
