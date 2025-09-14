using System.Security.Claims;

namespace BasicAuthentication.Services;

/// <summary>
/// Service interface for user authentication and management.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Validates user credentials and returns claims if valid.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <param name="password">The password to validate.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A list of claims if authentication is successful, null otherwise.</returns>
    Task<IEnumerable<Claim>?> ValidateCredentialsAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken = default);
}