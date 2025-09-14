namespace JwtAuthentication.Models;

/// <summary>
/// Represents a user login request.
/// </summary>
/// <param name="Username">The username for authentication.</param>
/// <param name="Password">The password for authentication.</param>
public record LoginRequest(string Username, string Password);

/// <summary>
/// Represents the response after successful login.
/// </summary>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="RefreshToken">The refresh token for obtaining new access tokens.</param>
/// <param name="TokenType">The type of token (typically "Bearer").</param>
/// <param name="ExpiresIn">The number of seconds until the access token expires.</param>
/// <param name="User">Information about the authenticated user.</param>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    UserInfo User
);

/// <summary>
/// Represents a request to refresh an access token.
/// </summary>
/// <param name="RefreshToken">The refresh token.</param>
public record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Represents the response after successful token refresh.
/// </summary>
/// <param name="AccessToken">The new JWT access token.</param>
/// <param name="RefreshToken">The new refresh token (if rotation is enabled).</param>
/// <param name="TokenType">The type of token (typically "Bearer").</param>
/// <param name="ExpiresIn">The number of seconds until the access token expires.</param>
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn
);

/// <summary>
/// Represents user information included in API responses.
/// </summary>
/// <param name="Username">The user's username.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Roles">The roles assigned to the user.</param>
public record UserInfo(
    string Username,
    string DisplayName,
    string? Email,
    string[] Roles
);

/// <summary>
/// Represents detailed user information for protected endpoints.
/// </summary>
/// <param name="Username">The username of the authenticated user.</param>
/// <param name="IsAuthenticated">Whether the user is authenticated.</param>
/// <param name="AuthenticationType">The type of authentication used.</param>
/// <param name="Roles">The roles assigned to the user.</param>
/// <param name="Claims">The claims associated with the user.</param>
/// <param name="RetrievedAt">When the user information was retrieved.</param>
public record UserInfoResponse(
    string Username,
    bool IsAuthenticated,
    string AuthenticationType,
    string[] Roles,
    ClaimInfo[] Claims,
    DateTimeOffset RetrievedAt
);

/// <summary>
/// Represents a claim for API responses.
/// </summary>
/// <param name="Type">The claim type.</param>
/// <param name="Value">The claim value.</param>
public record ClaimInfo(string Type, string Value);

/// <summary>
/// Represents a summary of a user for administrative endpoints.
/// </summary>
/// <param name="Username">The user's username.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Roles">The roles assigned to the user.</param>
public record UserSummary(string Username, string DisplayName, string[] Roles);

/// <summary>
/// Represents the response for user list endpoints.
/// </summary>
/// <param name="Users">The list of users.</param>
/// <param name="RequestedBy">The username of the user who requested the list.</param>
/// <param name="RetrievedAt">When the user list was retrieved.</param>
public record UsersResponse(
    UserSummary[] Users,
    string RequestedBy,
    DateTimeOffset RetrievedAt
);

/// <summary>
/// Represents error responses for authentication failures.
/// </summary>
/// <param name="Error">The error code.</param>
/// <param name="ErrorDescription">A human-readable description of the error.</param>
/// <param name="Timestamp">When the error occurred.</param>
public record AuthenticationError(
    string Error,
    string ErrorDescription,
    DateTimeOffset Timestamp
);