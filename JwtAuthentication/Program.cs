using JwtAuthentication.Models;
using JwtAuthentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults for .NET Aspire
builder.AddServiceDefaults();

// Configure JWT options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Configure services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();

// Get JWT configuration for authentication setup
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing");

// Configure authentication with multiple JWT Bearer schemes
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
    options.DefaultScheme = "Bearer";
})
.AddJwtBearer("JWT", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.FromMinutes(1) // Reduce clock skew
    };

    // Handle JWT events for better logging and debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var username = context.Principal?.Identity?.Name;
            logger.LogDebug("JWT token validated for user: {Username}", username);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("JWT authentication challenge issued for {Path}", context.Request.Path);
            return Task.CompletedTask;
        }
    };
})
.AddJwtBearer("DevJWT", options =>
{
    // Configure for dotnet user-jwts tokens in development
    if (builder.Environment.IsDevelopment())
    {
        var devJwtSettings = builder.Configuration.GetSection("Authentication:Schemes:Bearer");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false, // user-jwts uses different signing
            ValidIssuers = new[] { "dotnet-user-jwts" },
            ValidAudiences = devJwtSettings.GetSection("ValidAudiences").Get<string[]>() ?? [],
            SignatureValidator = (token, parameters) => new JwtSecurityToken(token), // Skip signature validation for dev tokens
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    }
})
.AddPolicyScheme("Bearer", "Bearer", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader["Bearer ".Length..].Trim();
            
            // Simple check to determine which scheme to use
            // In a real scenario, you might decode the token to check the issuer
            if (builder.Environment.IsDevelopment() && token.Contains("dotnet-user-jwts"))
            {
                return "DevJWT";
            }
        }
        return "JWT";
    };
});

// Add authorization services
builder.Services.AddAuthorization();

// Add OpenAPI/Swagger with security definitions
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWT Authentication Demo API",
        Version = "v1",
        Description = "Demonstrates JWT Bearer Authentication in ASP.NET Core with minimal APIs"
    });

    // Add JWT Bearer security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token in the format: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "JWT Auth Demo API v1");
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

// Group authentication endpoints
var authApi = app.MapGroup("/api/auth")
    .WithTags("Authentication")
    .WithOpenApi();

authApi.MapPost("/login", Login)
    .WithName("Login")
    .WithSummary("User login")
    .WithDescription("Authenticates user credentials and returns JWT tokens")
    .Accepts<LoginRequest>("application/json")
    .Produces<LoginResponse>(200)
    .Produces<AuthenticationError>(401);

authApi.MapPost("/refresh", RefreshToken)
    .WithName("RefreshToken")
    .WithSummary("Refresh access token")
    .WithDescription("Exchanges a valid refresh token for a new access token")
    .Accepts<RefreshTokenRequest>("application/json")
    .Produces<RefreshTokenResponse>(200)
    .Produces<AuthenticationError>(401);

authApi.MapPost("/logout", Logout)
    .WithName("Logout")
    .WithSummary("User logout")
    .WithDescription("Revokes the current refresh token")
    .Accepts<RefreshTokenRequest>("application/json")
    .Produces(200)
    .RequireAuthorization();

// Group protected endpoints
var protectedApi = app.MapGroup("/api/protected")
    .WithTags("Protected")
    .WithOpenApi()
    .RequireAuthorization();

protectedApi.MapGet("/weather", GetProtectedWeatherForecast)
    .WithName("GetProtectedWeather")
    .WithSummary("Get weather forecast (protected)")
    .WithDescription("Returns extended weather forecast data - requires JWT authentication");

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

static async Task<IResult> Login(
    LoginRequest request, 
    IUserService userService, 
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    ILogger<Program> logger)
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        var error = new AuthenticationError(
            "invalid_request",
            "Username and password are required",
            DateTimeOffset.UtcNow
        );
        return TypedResults.BadRequest(error);
    }

    var claims = await userService.ValidateCredentialsAsync(request.Username, request.Password);
    
    if (claims == null)
    {
        var error = new AuthenticationError(
            "invalid_credentials",
            "Invalid username or password",
            DateTimeOffset.UtcNow
        );
        return TypedResults.Unauthorized();
    }

    // Generate tokens
    var accessToken = jwtTokenService.GenerateAccessToken(claims);
    var refreshToken = jwtTokenService.GenerateRefreshToken();
    
    // Store refresh token
    var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // TODO: Get from configuration
    await refreshTokenService.StoreRefreshTokenAsync(request.Username, refreshToken, refreshTokenExpiry);

    // Create user info from claims
    var userInfo = new UserInfo(
        Username: request.Username,
        DisplayName: claims.FirstOrDefault(c => c.Type == "display_name")?.Value ?? request.Username,
        Email: claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
        Roles: claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray()
    );

    var response = new LoginResponse(
        AccessToken: accessToken,
        RefreshToken: refreshToken,
        TokenType: "Bearer",
        ExpiresIn: jwtTokenService.GetAccessTokenExpiryInSeconds(),
        User: userInfo
    );

    logger.LogInformation("User {Username} logged in successfully", request.Username);
    
    return TypedResults.Ok(response);
}

static async Task<IResult> RefreshToken(
    RefreshTokenRequest request,
    IUserService userService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    ILogger<Program> logger)
{
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
    {
        var error = new AuthenticationError(
            "invalid_request",
            "Refresh token is required",
            DateTimeOffset.UtcNow
        );
        return TypedResults.BadRequest(error);
    }

    var username = await refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken);
    
    if (username == null)
    {
        var error = new AuthenticationError(
            "invalid_token",
            "Invalid or expired refresh token",
            DateTimeOffset.UtcNow
        );
        return TypedResults.Unauthorized();
    }

    // Get user claims
    var claims = await userService.GetUserByUsernameAsync(username);
    
    if (claims == null)
    {
        var error = new AuthenticationError(
            "user_not_found",
            "User associated with refresh token no longer exists",
            DateTimeOffset.UtcNow
        );
        return TypedResults.Unauthorized();
    }

    // Generate new tokens
    var newAccessToken = jwtTokenService.GenerateAccessToken(claims);
    var newRefreshToken = jwtTokenService.GenerateRefreshToken();
    
    // Revoke old refresh token and store new one
    await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
    var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // TODO: Get from configuration
    await refreshTokenService.StoreRefreshTokenAsync(username, newRefreshToken, refreshTokenExpiry);

    var response = new RefreshTokenResponse(
        AccessToken: newAccessToken,
        RefreshToken: newRefreshToken,
        TokenType: "Bearer",
        ExpiresIn: jwtTokenService.GetAccessTokenExpiryInSeconds()
    );

    logger.LogInformation("Tokens refreshed for user {Username}", username);
    
    return TypedResults.Ok(response);
}

static async Task<IResult> Logout(
    RefreshTokenRequest request,
    IRefreshTokenService refreshTokenService,
    ILogger<Program> logger)
{
    if (!string.IsNullOrWhiteSpace(request.RefreshToken))
    {
        await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        logger.LogInformation("User logged out and refresh token revoked");
    }

    return TypedResults.Ok(new { message = "Logged out successfully" });
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

static IResult GetAllUsers(ClaimsPrincipal user)
{
    // This is a mock implementation - in a real app, you'd have a method to get all users
    var users = new[]
    {
        new UserSummary("admin", "Administrator", ["Admin", "User"]),
        new UserSummary("user", "Regular User", ["User"]),
        new UserSummary("test", "Test User", ["User"]),
        new UserSummary("demo", "Demo User", ["User"]),
        new UserSummary("manager", "Department Manager", ["Manager", "User"])
    };

    var response = new UsersResponse(
        Users: users,
        RequestedBy: user.Identity?.Name ?? "Unknown",
        RetrievedAt: DateTimeOffset.UtcNow
    );

    return TypedResults.Ok(response);
}