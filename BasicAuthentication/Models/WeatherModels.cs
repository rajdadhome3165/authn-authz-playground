namespace BasicAuthentication.Models;

/// <summary>
/// Represents a weather forecast for a specific date.
/// </summary>
/// <param name="Date">The date for the forecast</param>
/// <param name="TemperatureC">Temperature in Celsius</param>
/// <param name="Summary">Weather description</param>
public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary
)
{
    /// <summary>
    /// Gets the temperature in Fahrenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

/// <summary>
/// Represents a complete weather forecast response.
/// </summary>
/// <param name="Forecasts">Collection of weather forecasts</param>
/// <param name="GeneratedAt">When the forecast was generated</param>
/// <param name="Source">The source of the forecast data</param>
public record WeatherForecastResponse(
    WeatherForecast[] Forecasts,
    DateTimeOffset GeneratedAt,
    string Source
);