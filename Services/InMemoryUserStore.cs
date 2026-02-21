using Auth.API.Models;

namespace Auth.API.Services;

/// <summary>
/// In-memory user store implementation
/// </summary>
public class InMemoryUserStore : IUserStore
{
    private readonly List<User> _users;
    private readonly ILogger<InMemoryUserStore> _logger;
    private int _nextUserId = 1;

    public InMemoryUserStore(ILogger<InMemoryUserStore> logger)
    {
        _logger = logger;
        _users = new List<User>();
        InitializeDefaultUsers();
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<List<User>> GetAllUsersAsync()
    {
        return Task.FromResult(new List<User>(_users));
    }

    public Task<User> AddUserAsync(User user)
    {
        user.Id = _nextUserId++;
        _users.Add(user);
        _logger.LogInformation($"User added: {user.Username}");
        return Task.FromResult(user);
    }

    private void InitializeDefaultUsers()
    {
        var defaultUsers = new[]
        {
            new User
            {
                Id = _nextUserId++,
                Username = "admin",
                Password = "admin123",
                Email = "admin@example.com",
                Roles = new List<string> { "Admin", "User" }
            },
            new User
            {
                Id = _nextUserId++,
                Username = "user",
                Password = "user123",
                Email = "user@example.com",
                Roles = new List<string> { "User" }
            },
            new User
            {
                Id = _nextUserId++,
                Username = "moderator",
                Password = "moderator123",
                Email = "moderator@example.com",
                Roles = new List<string> { "Moderator", "User" }
            }
        };

        foreach (var user in defaultUsers)
        {
            _users.Add(user);
        }

        _logger.LogInformation("In-memory user store initialized with default users");
    }
}
