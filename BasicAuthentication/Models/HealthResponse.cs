namespace BasicAuthentication.Models;

/// <summary>
/// Represents the health status response for the application.
/// </summary>
/// <param name="Status">The current health status of the application</param>
/// <param name="Timestamp">When the health check was performed</param>
/// <param name="Version">The application version</param>
/// <param name="Environment">The current environment (Development, Production, etc.)</param>
public record HealthResponse(
    string Status,
    DateTimeOffset Timestamp,
    string Version,
    string Environment
);