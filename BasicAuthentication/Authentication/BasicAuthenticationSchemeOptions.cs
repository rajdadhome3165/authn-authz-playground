using Microsoft.AspNetCore.Authentication;

namespace BasicAuthentication.Authentication;

/// <summary>
/// Options for configuring Basic Authentication scheme.
/// </summary>
public class BasicAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets or sets the realm name for Basic Authentication.
    /// This value is included in the WWW-Authenticate header when challenging users.
    /// </summary>
    public string Realm { get; set; } = "Basic Authentication";
}