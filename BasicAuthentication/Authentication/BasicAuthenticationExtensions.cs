using BasicAuthentication.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring Basic Authentication services.
/// </summary>
public static class BasicAuthenticationExtensions
{
    /// <summary>
    /// The default scheme name for Basic Authentication.
    /// </summary>
    public const string DefaultScheme = "BasicAuthentication";

    /// <summary>
    /// Adds Basic Authentication to the authentication services.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <returns>The authentication builder for chaining.</returns>
    public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder)
        => builder.AddBasicAuthentication(DefaultScheme);

    /// <summary>
    /// Adds Basic Authentication to the authentication services with a custom scheme.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="scheme">The authentication scheme name.</param>
    /// <returns>The authentication builder for chaining.</returns>
    public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder, string scheme)
        => builder.AddBasicAuthentication(scheme, options => { });

    /// <summary>
    /// Adds Basic Authentication to the authentication services with configuration.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="configureOptions">Action to configure the Basic Authentication options.</param>
    /// <returns>The authentication builder for chaining.</returns>
    public static AuthenticationBuilder AddBasicAuthentication(
        this AuthenticationBuilder builder,
        Action<BasicAuthenticationSchemeOptions> configureOptions)
        => builder.AddBasicAuthentication(DefaultScheme, configureOptions);

    /// <summary>
    /// Adds Basic Authentication to the authentication services with a custom scheme and configuration.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="scheme">The authentication scheme name.</param>
    /// <param name="configureOptions">Action to configure the Basic Authentication options.</param>
    /// <returns>The authentication builder for chaining.</returns>
    public static AuthenticationBuilder AddBasicAuthentication(
        this AuthenticationBuilder builder,
        string scheme,
        Action<BasicAuthenticationSchemeOptions> configureOptions)
    {
        return builder.AddScheme<BasicAuthenticationSchemeOptions, BasicAuthenticationHandler>(
            scheme, configureOptions);
    }
}