using System.Security.Claims;

namespace JwtAuthentication.Services;

/// <summary>
/// Interface for user authentication and management services.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Validates user credentials asynchronously.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <param name="password">The password to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the user claims if validation succeeds, otherwise null.
    /// </returns>
    Task<IEnumerable<Claim>?> ValidateCredentialsAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user information by username.
    /// </summary>
    /// <param name="username">The username to lookup.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains user claims if the user exists, otherwise null.
    /// </returns>
    Task<IEnumerable<Claim>?> GetUserByUsernameAsync(
        string username, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for JWT token management services.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user claims.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <returns>The generated JWT access token.</returns>
    string GenerateAccessToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>A new refresh token.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns the principal if valid.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>The claims principal if the token is valid, otherwise null.</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Gets the expiry time in seconds for access tokens.
    /// </summary>
    /// <returns>The number of seconds until access tokens expire.</returns>
    int GetAccessTokenExpiryInSeconds();
}

/// <summary>
/// Interface for refresh token management services.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Stores a refresh token for a user.
    /// </summary>
    /// <param name="username">The username associated with the token.</param>
    /// <param name="refreshToken">The refresh token to store.</param>
    /// <param name="expiryDate">When the refresh token expires.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreRefreshTokenAsync(
        string username, 
        string refreshToken, 
        DateTime expiryDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a refresh token and returns the associated username.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the username if the token is valid, otherwise null.
    /// </returns>
    Task<string?> ValidateRefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeRefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    /// <param name="username">The username whose tokens should be revoked.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeUserTokensAsync(
        string username, 
        CancellationToken cancellationToken = default);
}