using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace JwtAuthentication.Services;

/// <summary>
/// In-memory refresh token service for demonstration purposes.
/// In production, this should be replaced with a database or distributed cache implementation.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly ILogger<RefreshTokenService> logger;
    private readonly JwtOptions jwtOptions;
    
    // In-memory storage for refresh tokens
    // In production, use a database or distributed cache like Redis
    private readonly ConcurrentDictionary<string, RefreshTokenData> refreshTokens = new();

    public RefreshTokenService(ILogger<RefreshTokenService> logger, IOptions<JwtOptions> jwtOptions)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    /// <inheritdoc />
    public Task StoreRefreshTokenAsync(
        string username, 
        string refreshToken, 
        DateTime expiryDate, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be null or empty", nameof(refreshToken));

        var tokenData = new RefreshTokenData(username, expiryDate, DateTime.UtcNow);
        refreshTokens[refreshToken] = tokenData;

        logger.LogDebug("Stored refresh token for user {Username} with expiry {Expiry}", 
            username, expiryDate);

        // Clean up expired tokens periodically (simple approach for demo)
        _ = Task.Run(CleanupExpiredTokens, cancellationToken);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string?> ValidateRefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            logger.LogWarning("Refresh token validation attempted with empty token");
            return Task.FromResult<string?>(null);
        }

        if (refreshTokens.TryGetValue(refreshToken, out var tokenData))
        {
            if (tokenData.ExpiryDate > DateTime.UtcNow)
            {
                logger.LogDebug("Refresh token validated successfully for user {Username}", 
                    tokenData.Username);
                return Task.FromResult<string?>(tokenData.Username);
            }
            else
            {
                logger.LogWarning("Refresh token for user {Username} has expired", 
                    tokenData.Username);
                
                // Remove expired token
                refreshTokens.TryRemove(refreshToken, out _);
                return Task.FromResult<string?>(null);
            }
        }

        logger.LogWarning("Refresh token validation failed: Token not found");
        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public Task RevokeRefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Task.CompletedTask;
        }

        if (refreshTokens.TryRemove(refreshToken, out var tokenData))
        {
            logger.LogInformation("Revoked refresh token for user {Username}", 
                tokenData.Username);
        }
        else
        {
            logger.LogDebug("Attempted to revoke non-existent refresh token");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RevokeUserTokensAsync(
        string username, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.CompletedTask;
        }

        var revokedCount = 0;
        var tokensToRemove = refreshTokens
            .Where(kvp => kvp.Value.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in tokensToRemove)
        {
            if (refreshTokens.TryRemove(token, out _))
            {
                revokedCount++;
            }
        }

        logger.LogInformation("Revoked {Count} refresh tokens for user {Username}", 
            revokedCount, username);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes expired refresh tokens from memory.
    /// In production, this would be handled by the database or cache system.
    /// </summary>
    private void CleanupExpiredTokens()
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiredTokens = refreshTokens
                .Where(kvp => kvp.Value.ExpiryDate <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var token in expiredTokens)
            {
                refreshTokens.TryRemove(token, out _);
            }

            if (expiredTokens.Count > 0)
            {
                logger.LogDebug("Cleaned up {Count} expired refresh tokens", expiredTokens.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during refresh token cleanup");
        }
    }

    /// <summary>
    /// Represents refresh token data stored in memory.
    /// </summary>
    private record RefreshTokenData(string Username, DateTime ExpiryDate, DateTime CreatedAt);
}