using BasicAuthentication.Authentication;
using BasicAuthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults for .NET Aspire
builder.AddServiceDefaults();

// Configure services
builder.Services.AddScoped<IUserService, UserService>();

// Configure authentication with Basic Authentication
builder.Services.AddAuthentication("BasicAuthentication")
    .AddBasicAuthentication(options =>
    {
        options.Realm = "Basic Authentication Demo API";
    });

// Add authorization services
builder.Services.AddAuthorization();

// Add OpenAPI/Swagger with security definitions
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Basic Authentication Demo API",
        Version = "v1",
        Description = "Demonstrates HTTP Basic Authentication in ASP.NET Core with minimal APIs"
    });

    // Add Basic Auth security definition
    options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Enter username and password for Basic authentication"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Basic"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add problem details for better error handling
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

var app = builder.Build();

// Map Aspire default endpoints
app.MapDefaultEndpoints();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Basic Auth Demo API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

// Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    await next();
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Group public endpoints
var publicApi = app.MapGroup("/api/public")
    .WithTags("Public")
    .WithOpenApi();

publicApi.MapGet("/health", GetHealthStatus)
    .WithName("GetHealth")
    .WithSummary("Get application health status")
    .WithDescription("Returns the health status of the application - no authentication required");

publicApi.MapGet("/weather", GetWeatherForecast)
    .WithName("GetPublicWeather")
    .WithSummary("Get weather forecast (public)")
    .WithDescription("Returns weather forecast data - no authentication required");

// Group protected endpoints
var protectedApi = app.MapGroup("/api/protected")
    .WithTags("Protected")
    .WithOpenApi()
    .RequireAuthorization();

protectedApi.MapGet("/weather", GetProtectedWeatherForecast)
    .WithName("GetProtectedWeather")
    .WithSummary("Get weather forecast (protected)")
    .WithDescription("Returns extended weather forecast data - requires authentication");

protectedApi.MapGet("/user-info", GetUserInfo)
    .WithName("GetUserInfo")
    .WithSummary("Get current user information")
    .WithDescription("Returns information about the authenticated user");

// Admin-only endpoints
var adminApi = app.MapGroup("/api/admin")
    .WithTags("Admin")
    .WithOpenApi()
    .RequireAuthorization(policy => policy.RequireRole("Admin"));

adminApi.MapGet("/users", GetAllUsers)
    .WithName("GetAllUsers")
    .WithSummary("Get all users (admin only)")
    .WithDescription("Returns information about all users - requires Admin role");

// Legacy redirect for backward compatibility
app.MapGet("/", () => TypedResults.Redirect("/swagger"))
    .ExcludeFromDescription();

await app.RunAsync();

// Endpoint handlers
static IResult GetHealthStatus()
{
    var response = new HealthResponse(
        Status: "Healthy",
        Timestamp: DateTimeOffset.UtcNow,
        Version: "1.0.0",
        Environment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
    );

    return TypedResults.Ok(response);
}

static IResult GetWeatherForecast()
{
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    
    var forecasts = Enumerable.Range(1, 5)
        .Select(index => new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    var response = new WeatherForecastResponse(
        Forecasts: forecasts,
        GeneratedAt: DateTimeOffset.UtcNow,
        Source: "Public API"
    );

    return TypedResults.Ok(response);
}

static IResult GetProtectedWeatherForecast(ClaimsPrincipal user)
{
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    
    var forecasts = Enumerable.Range(1, 7) // More days for authenticated users
        .Select(index => new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    var response = new WeatherForecastResponse(
        Forecasts: forecasts,
        GeneratedAt: DateTimeOffset.UtcNow,
        Source: $"Protected API (User: {user.Identity?.Name})"
    );

    return TypedResults.Ok(response);
}

static IResult GetUserInfo(ClaimsPrincipal user)
{
    if (user.Identity is not { IsAuthenticated: true })
    {
        return TypedResults.Unauthorized();
    }

    var claims = user.Claims
        .Select(c => new ClaimInfo(c.Type, c.Value))
        .ToArray();

    var response = new UserInfoResponse(
        Username: user.Identity.Name ?? "Unknown",
        IsAuthenticated: user.Identity.IsAuthenticated,
        AuthenticationType: user.Identity.AuthenticationType ?? "Unknown",
        Roles: user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
        Claims: claims,
        RetrievedAt: DateTimeOffset.UtcNow
    );

    return TypedResults.Ok(response);
}

static IResult GetAllUsers(IUserService userService, ClaimsPrincipal user)
{
    // This is a mock implementation - in a real app, you'd have a method to get all users
    var users = new[]
    {
        new UserSummary("admin", "Administrator", ["Admin", "User"]),
        new UserSummary("user", "Regular User", ["User"]),
        new UserSummary("test", "Test User", ["User"]),
        new UserSummary("demo", "Demo User", ["User"])
    };

    var response = new UsersResponse(
        Users: users,
        RequestedBy: user.Identity?.Name ?? "Unknown",
        RetrievedAt: DateTimeOffset.UtcNow
    );

    return TypedResults.Ok(response);
}

// Response DTOs
public record HealthResponse(
    string Status,
    DateTimeOffset Timestamp,
    string Version,
    string Environment
);

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary
)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record WeatherForecastResponse(
    WeatherForecast[] Forecasts,
    DateTimeOffset GeneratedAt,
    string Source
);

public record ClaimInfo(
    string Type,
    string Value
);

public record UserInfoResponse(
    string Username,
    bool IsAuthenticated,
    string AuthenticationType,
    string[] Roles,
    ClaimInfo[] Claims,
    DateTimeOffset RetrievedAt
);

public record UserSummary(
    string Username,
    string DisplayName,
    string[] Roles
);

public record UsersResponse(
    UserSummary[] Users,
    string RequestedBy,
    DateTimeOffset RetrievedAt
);
