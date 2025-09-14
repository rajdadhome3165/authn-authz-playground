using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using BasicAuthentication.Services;

namespace BasicAuthentication.Authentication;

/// <summary>
/// Custom authentication handler for HTTP Basic Authentication.
/// Processes Authorization header with Basic scheme and validates credentials.
/// </summary>
public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationSchemeOptions>
{
    private readonly IUserService _userService;

    public BasicAuthenticationHandler(
        IOptionsMonitor<BasicAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserService userService)
        : base(options, logger, encoder)
    {
        _userService = userService;
    }

    /// <summary>
    /// Handles the authentication by processing the Authorization header.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if Authorization header exists
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            Logger.LogDebug("No Authorization header found in request");
            return AuthenticateResult.NoResult();
        }

        string? authorizationHeader = Request.Headers.Authorization;
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            Logger.LogDebug("Authorization header is empty or whitespace");
            return AuthenticateResult.NoResult();
        }

        // Check if it's Basic authentication scheme
        if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogDebug("Authorization header does not use Basic scheme: {Scheme}", 
                authorizationHeader.Split(' ').FirstOrDefault());
            return AuthenticateResult.NoResult();
        }

        // Extract and decode credentials
        var (username, password) = ExtractCredentials(authorizationHeader);
        if (username is null || password is null)
        {
            Logger.LogWarning("Invalid Authorization header format - unable to extract credentials");
            return AuthenticateResult.Fail("Invalid Authorization header format");
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Logger.LogWarning("Empty username or password provided");
            return AuthenticateResult.Fail("Username and password cannot be empty");
        }

        try
        {
            // Validate credentials using the user service
            var claims = await _userService.ValidateCredentialsAsync(username, password, Context.RequestAborted);
            
            if (claims is null)
            {
                Logger.LogWarning("Authentication failed for user: {Username}", username);
                return AuthenticateResult.Fail("Invalid username or password");
            }

            // Create authentication ticket
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation("User {Username} authenticated successfully via Basic Authentication", username);
            return AuthenticateResult.Success(ticket);
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Authentication cancelled for user: {Username}", username);
            return AuthenticateResult.Fail("Authentication request was cancelled");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during authentication for user: {Username}", username);
            return AuthenticateResult.Fail("An error occurred during authentication");
        }
    }

    /// <summary>
    /// Handles authentication challenges by returning 401 with WWW-Authenticate header.
    /// </summary>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = $"Basic realm=\"{Options.Realm}\"";
        return base.HandleChallengeAsync(properties);
    }

    /// <summary>
    /// Extracts username and password from the Basic Authentication header.
    /// </summary>
    /// <param name="authorizationHeader">The Authorization header value.</param>
    /// <returns>A tuple containing username and password, or null values if invalid.</returns>
    private static (string? Username, string? Password) ExtractCredentials(string authorizationHeader)
    {
        try
        {
            // Remove "Basic " prefix (7 characters)
            if (authorizationHeader.Length <= 6)
            {
                return (null, null);
            }

            var encodedCredentials = authorizationHeader[6..].Trim();
            
            if (string.IsNullOrWhiteSpace(encodedCredentials))
            {
                return (null, null);
            }
            
            // Decode base64
            byte[] decodedBytes;
            try
            {
                decodedBytes = Convert.FromBase64String(encodedCredentials);
            }
            catch (FormatException)
            {
                // Invalid base64 encoding
                return (null, null);
            }

            var decodedCredentials = Encoding.UTF8.GetString(decodedBytes);
            
            // Split by first colon (username:password)
            var colonIndex = decodedCredentials.IndexOf(':');
            if (colonIndex == -1)
            {
                // No colon found - invalid format
                return (null, null);
            }

            var username = decodedCredentials[..colonIndex];
            var password = decodedCredentials[(colonIndex + 1)..];

            return (username, password);
        }
        catch (Exception)
        {
            // Any other unexpected error
            return (null, null);
        }
    }
}