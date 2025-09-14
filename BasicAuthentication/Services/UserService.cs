using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BasicAuthentication.Services;

/// <summary>
/// Simple in-memory user service for demonstration purposes.
/// In a real application, this would validate against a database or external identity provider.
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    
    // Demo users - in production, this would come from a database or external service
    // Passwords should be hashed in production, but kept plain for demo simplicity
    private readonly Dictionary<string, UserData> _users = new()
    {
        ["admin"] = new("admin123", ["Admin", "User"], "Administrator"),
        ["user"] = new("user123", ["User"], "Regular User"),
        ["test"] = new("test123", ["User"], "Test User"),
        ["demo"] = new("demo123", ["User"], "Demo User")
    };

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<IEnumerable<Claim>?> ValidateCredentialsAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // Add artificial delay to prevent timing attacks (in production, use proper password hashing)
        Task.Delay(Random.Shared.Next(50, 150), cancellationToken).Wait(cancellationToken);

        _logger.LogDebug("Validating credentials for user: {Username}", username);

        if (!_users.TryGetValue(username, out var userData))
        {
            _logger.LogWarning("Authentication failed: User '{Username}' not found", username);
            return Task.FromResult<IEnumerable<Claim>?>(null);
        }

        // In production, use secure password comparison (e.g., BCrypt.Verify)
        if (!SecureStringCompare(userData.Password, password))
        {
            _logger.LogWarning("Authentication failed: Invalid password for user '{Username}'", username);
            return Task.FromResult<IEnumerable<Claim>?>(null);
        }

        _logger.LogInformation("User '{Username}' authenticated successfully", username);

        var claims = CreateUserClaims(username, userData);
        return Task.FromResult<IEnumerable<Claim>?>(claims);
    }

    /// <summary>
    /// Creates claims for an authenticated user.
    /// </summary>
    private static IEnumerable<Claim> CreateUserClaims(string username, UserData userData)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, username),
            new("sub", username), // Standard subject claim (OIDC)
            new("preferred_username", username),
            new("display_name", userData.DisplayName),
            new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // Issued at
        };

        // Add role claims
        foreach (var role in userData.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    /// <summary>
    /// Performs a secure string comparison to prevent timing attacks.
    /// In production, use proper password hashing (e.g., BCrypt, Argon2).
    /// </summary>
    private static bool SecureStringCompare(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

    /// <summary>
    /// Represents user data for authentication.
    /// </summary>
    private record UserData(string Password, string[] Roles, string DisplayName);
}