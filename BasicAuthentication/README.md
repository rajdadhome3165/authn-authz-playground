# Basic Authentication Demo

A comprehensive demonstration of HTTP Basic Authentication implementation in ASP.NET Core 9 using minimal APIs and modern C# patterns.

## Overview

This project showcases how to implement HTTP Basic Authentication in ASP.NET Core following current best practices. It includes:

- Custom `AuthenticationHandler` implementation
- Structured API responses using record types
- Comprehensive Swagger/OpenAPI documentation with security definitions
- Role-based authorization
- Modern minimal API patterns with `MapGroup` and `TypedResults`
- Security headers and error handling middleware

## Features

### Authentication & Authorization
- **HTTP Basic Authentication**: Custom implementation using `AuthenticationHandler<T>`
- **Role-based Authorization**: Support for Admin and User roles
- **Claims-based Identity**: Rich user claims for authorization decisions
- **Secure Credential Validation**: Timing attack prevention and secure string comparison

### API Endpoints

#### Public Endpoints (No Authentication Required)
- `GET /api/public/health` - Application health status
- `GET /api/public/weather` - Public weather forecast (5 days)

#### Protected Endpoints (Authentication Required)
- `GET /api/protected/weather` - Extended weather forecast (7 days)
- `GET /api/protected/user-info` - Current user information and claims

#### Admin Endpoints (Admin Role Required)
- `GET /api/admin/users` - List all users (Admin role required)

### Modern ASP.NET Core Patterns
- **Minimal APIs**: Clean endpoint definitions with `MapGroup`
- **TypedResults**: Strongly-typed HTTP responses
- **Record Types**: Immutable response DTOs
- **Dependency Injection**: Scoped services and proper DI patterns
- **Problem Details**: Standardized error responses (RFC 7807)
- **Security Headers**: OWASP-recommended security headers

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code
- Basic understanding of ASP.NET Core and HTTP authentication

### Demo Users

The application includes these pre-configured demo users for testing:

| Username | Password | Roles | Description |
|----------|----------|-------|-------------|
| `admin` | `admin123` | Admin, User | Administrator with full access |
| `user` | `user123` | User | Regular user |
| `test` | `test123` | User | Test user |
| `demo` | `demo123` | User | Demo user |

> **Note**: In production, passwords should be properly hashed using bcrypt, Argon2, or similar algorithms.

### Running the Application

1. **Clone and Navigate**:
   ```bash
   cd BasicAuthentication
   ```

2. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the Application**:
   ```bash
   dotnet run
   ```

4. **Access Swagger UI**:
   - Open browser to `https://localhost:5001` or `http://localhost:5000`
   - The Swagger UI will be displayed at the root URL

### Testing Authentication

#### Using Swagger UI
1. Navigate to the Swagger UI at the root URL
2. Click on any protected endpoint (🔒 icon)
3. Click "Authorize" button
4. Enter username and password from the demo users table
5. Execute the request

#### Using HTTP Clients

**cURL Examples**:

```bash
# Public endpoint (no auth required)
curl -X GET "https://localhost:5001/api/public/health"

# Protected endpoint with authentication
curl -X GET "https://localhost:5001/api/protected/user-info" \
     -H "Authorization: Basic YWRtaW46YWRtaW4xMjM="

# Admin endpoint (requires Admin role)
curl -X GET "https://localhost:5001/api/admin/users" \
     -H "Authorization: Basic YWRtaW46YWRtaW4xMjM="
```

**PowerShell Examples**:

```powershell
# Public endpoint
Invoke-RestMethod -Uri "https://localhost:5001/api/public/health" -Method Get

# Protected endpoint with authentication
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123"))
$headers = @{ Authorization = "Basic $credentials" }
Invoke-RestMethod -Uri "https://localhost:5001/api/protected/user-info" -Headers $headers -Method Get
```

## Project Structure

```
BasicAuthentication/
├── Authentication/
│   ├── BasicAuthenticationHandler.cs        # Core authentication logic
│   ├── BasicAuthenticationSchemeOptions.cs  # Configuration options
│   └── BasicAuthenticationExtensions.cs     # Service registration extensions
├── Services/
│   ├── IUserService.cs                      # User service interface
│   └── UserService.cs                       # User validation implementation
├── Program.cs                               # Application configuration
├── BasicAuthentication.csproj              # Project file
└── README.md                               # This file
```

## Key Components

### BasicAuthenticationHandler
Custom authentication handler that:
- Extracts credentials from Authorization header
- Validates username/password using IUserService
- Creates ClaimsPrincipal with user identity and roles
- Implements comprehensive error handling and logging

### IUserService & UserService
Service responsible for:
- User credential validation
- Claims generation
- Role assignment
- Timing attack prevention

### Security Features
- **Timing Attack Prevention**: Consistent validation time regardless of user existence
- **Secure String Comparison**: Constant-time string comparison for passwords
- **Security Headers**: OWASP-recommended headers (X-Content-Type-Options, X-Frame-Options, etc.)
- **HTTPS Redirection**: Automatic redirect to HTTPS in production

## Configuration

### Authentication Options
```csharp
builder.Services.AddAuthentication("BasicAuthentication")
    .AddBasicAuthentication(options =>
    {
        options.Realm = "Basic Authentication Demo API";
    });
```

### Authorization Policies
```csharp
// Role-based authorization
.RequireAuthorization(policy => policy.RequireRole("Admin"))
```

## Production Considerations

### Security Enhancements
1. **Password Hashing**: Implement proper password hashing (bcrypt, Argon2)
2. **Rate Limiting**: Add rate limiting to prevent brute force attacks
3. **HTTPS Only**: Enforce HTTPS in production
4. **Audit Logging**: Log authentication attempts and security events
5. **Token Expiration**: Consider implementing session timeouts

### Database Integration
Replace the in-memory user store with:
- Entity Framework Core for relational databases
- MongoDB for document storage
- External identity providers (Azure AD, Auth0, etc.)

### Performance Optimizations
- Cache user validation results
- Implement connection pooling for database queries
- Use async/await patterns consistently

## Dependencies

- **Microsoft.AspNetCore.OpenApi** (9.0.8): OpenAPI support
- **Swashbuckle.AspNetCore** (7.2.0): Swagger UI and documentation
- **.NET Aspire ServiceDefaults**: Telemetry and service discovery

## Contributing

1. Follow the existing code style and patterns
2. Add unit tests for new functionality
3. Update documentation for any API changes
4. Ensure all security best practices are maintained

## License

This is a demonstration project for educational purposes.

## Related Resources

- [ASP.NET Core Authentication Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Minimal APIs in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [HTTP Basic Authentication RFC](https://tools.ietf.org/html/rfc7617)
- [OWASP Authentication Guidelines](https://owasp.org/www-project-cheat-sheets/cheatsheets/Authentication_Cheat_Sheet.html)