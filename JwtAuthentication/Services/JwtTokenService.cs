using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthentication.Services;

/// <summary>
/// Configuration options for JWT tokens.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Gets or sets the secret key used for signing JWT tokens.
    /// In production, this should come from a secure key store like Azure Key Vault.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issuer of the JWT tokens.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the audience for the JWT tokens.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiry time in minutes for access tokens.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the expiry time in days for refresh tokens.
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

/// <summary>
/// JWT token service implementation for generating and validating JWT tokens.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions jwtOptions;
    private readonly ILogger<JwtTokenService> logger;
    private readonly TokenValidationParameters tokenValidationParameters;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions, ILogger<JwtTokenService> logger)
    {
        this.jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate configuration
        if (string.IsNullOrWhiteSpace(this.jwtOptions.Key))
            throw new ArgumentException("JWT Key cannot be null or empty", nameof(jwtOptions));
        if (string.IsNullOrWhiteSpace(this.jwtOptions.Issuer))
            throw new ArgumentException("JWT Issuer cannot be null or empty", nameof(jwtOptions));
        if (string.IsNullOrWhiteSpace(this.jwtOptions.Audience))
            throw new ArgumentException("JWT Audience cannot be null or empty", nameof(jwtOptions));

        // Create token validation parameters
        tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = this.jwtOptions.Issuer,
            ValidAudience = this.jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtOptions.Key)),
            ClockSkew = TimeSpan.FromMinutes(1) // Reduce clock skew to 1 minute
        };
    }

    /// <inheritdoc />
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        logger.LogDebug("Generated access token for user with expiry {Expiry}", 
            token.ValidTo);

        return tokenString;
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        // Generate a cryptographically secure random string for the refresh token
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        var refreshToken = Convert.ToBase64String(randomBytes);
        
        logger.LogDebug("Generated new refresh token");
        
        return refreshToken;
    }

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            
            // Ensure the token is a JWT token
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogWarning("Invalid token format or algorithm");
                return null;
            }

            return principal;
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during token validation");
            return null;
        }
    }

    /// <inheritdoc />
    public int GetAccessTokenExpiryInSeconds()
    {
        return jwtOptions.ExpiryMinutes * 60;
    }
}