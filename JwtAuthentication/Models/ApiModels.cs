namespace JwtAuthentication.Models;

/// <summary>
/// Represents the health status of the application.
/// </summary>
/// <param name="Status">The current health status.</param>
/// <param name="Timestamp">When the health check was performed.</param>
/// <param name="Version">The application version.</param>
/// <param name="Environment">The current environment.</param>
public record HealthResponse(
    string Status,
    DateTimeOffset Timestamp,
    string Version,
    string Environment
);

/// <summary>
/// Represents a weather forecast entry.
/// </summary>
/// <param name="Date">The date of the forecast.</param>
/// <param name="TemperatureC">The temperature in Celsius.</param>
/// <param name="Summary">A summary of the weather conditions.</param>
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// Gets the temperature in Fahrenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

/// <summary>
/// Represents a weather forecast response with metadata.
/// </summary>
/// <param name="Forecasts">The weather forecast entries.</param>
/// <param name="GeneratedAt">When the forecast was generated.</param>
/// <param name="Source">The source of the forecast data.</param>
public record WeatherForecastResponse(
    WeatherForecast[] Forecasts,
    DateTimeOffset GeneratedAt,
    string Source
);