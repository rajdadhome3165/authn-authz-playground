using System.Security.Claims;

namespace JwtAuthentication.Services;

/// <summary>
/// Simple in-memory user service for demonstration purposes.
/// In a real application, this would validate against a database or external identity provider.
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> logger;
    
    // Demo users - in production, this would come from a database or external service
    // Passwords should be hashed in production, but kept plain for demo simplicity
    private readonly Dictionary<string, UserData> users = new()
    {
        ["admin"] = new("admin123", ["Admin", "User"], "Administrator", "admin@example.com"),
        ["user"] = new("user123", ["User"], "Regular User", "user@example.com"),
        ["test"] = new("test123", ["User"], "Test User", "test@example.com"),
        ["demo"] = new("demo123", ["User"], "Demo User", "demo@example.com"),
        ["manager"] = new("manager123", ["Manager", "User"], "Department Manager", "manager@example.com")
    };

    public UserService(ILogger<UserService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<IEnumerable<Claim>?> ValidateCredentialsAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Authentication attempt with empty username or password");
            return Task.FromResult<IEnumerable<Claim>?>(null);
        }

        // Convert to lowercase for case-insensitive comparison
        var normalizedUsername = username.ToLowerInvariant();
        
        if (users.TryGetValue(normalizedUsername, out var userData))
        {
            // Use secure string comparison to prevent timing attacks
            if (SecureStringCompare(password, userData.Password))
            {
                logger.LogInformation("User {Username} authenticated successfully", username);
                var claims = CreateUserClaims(normalizedUsername, userData);
                return Task.FromResult<IEnumerable<Claim>?>(claims);
            }
            else
            {
                logger.LogWarning("Authentication failed for user {Username}: Invalid password", username);
            }
        }
        else
        {
            logger.LogWarning("Authentication failed for user {Username}: User not found", username);
            
            // Perform dummy password check to prevent timing attacks
            SecureStringCompare(password, "dummy_password_to_prevent_timing_attacks");
        }

        return Task.FromResult<IEnumerable<Claim>?>(null);
    }

    /// <inheritdoc />
    public Task<IEnumerable<Claim>?> GetUserByUsernameAsync(
        string username, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult<IEnumerable<Claim>?>(null);
        }

        var normalizedUsername = username.ToLowerInvariant();
        
        if (users.TryGetValue(normalizedUsername, out var userData))
        {
            var claims = CreateUserClaims(normalizedUsername, userData);
            return Task.FromResult<IEnumerable<Claim>?>(claims);
        }

        return Task.FromResult<IEnumerable<Claim>?>(null);
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
            new(ClaimTypes.Email, userData.Email ?? $"{username}@example.com"),
            new("sub", username), // Standard subject claim (OIDC)
            new("preferred_username", username),
            new("display_name", userData.DisplayName),
            new("email", userData.Email ?? $"{username}@example.com"),
            new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), // Issued at
            new("jti", Guid.NewGuid().ToString()) // JWT ID
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
    private record UserData(string Password, string[] Roles, string DisplayName, string? Email);
}