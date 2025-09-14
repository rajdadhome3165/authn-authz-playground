namespace BasicAuthentication.Models;

/// <summary>
/// Represents a claim with its type and value.
/// </summary>
/// <param name="Type">The claim type</param>
/// <param name="Value">The claim value</param>
public record ClaimInfo(
    string Type,
    string Value
);

/// <summary>
/// Represents detailed information about the authenticated user.
/// </summary>
/// <param name="Username">The username of the user</param>
/// <param name="IsAuthenticated">Whether the user is authenticated</param>
/// <param name="AuthenticationType">The type of authentication used</param>
/// <param name="Roles">The roles assigned to the user</param>
/// <param name="Claims">All claims associated with the user</param>
/// <param name="RetrievedAt">When the user information was retrieved</param>
public record UserInfoResponse(
    string Username,
    bool IsAuthenticated,
    string AuthenticationType,
    string[] Roles,
    ClaimInfo[] Claims,
    DateTimeOffset RetrievedAt
);

/// <summary>
/// Represents a summary of user information.
/// </summary>
/// <param name="Username">The username</param>
/// <param name="DisplayName">The display name</param>
/// <param name="Roles">The roles assigned to the user</param>
public record UserSummary(
    string Username,
    string DisplayName,
    string[] Roles
);

/// <summary>
/// Represents a response containing multiple users.
/// </summary>
/// <param name="Users">Collection of user summaries</param>
/// <param name="RequestedBy">The user who requested this information</param>
/// <param name="RetrievedAt">When the user list was retrieved</param>
public record UsersResponse(
    UserSummary[] Users,
    string RequestedBy,
    DateTimeOffset RetrievedAt
);