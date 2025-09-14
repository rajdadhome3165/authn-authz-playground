# JWT Authentication Demo

A comprehensive demonstration of JWT (JSON Web Token) Bearer Authentication implementation in ASP.NET Core 9 using minimal APIs, modern C# patterns, and the latest security best practices.

## Overview

This project showcases how to implement JWT Bearer Authentication in ASP.NET Core following current best practices. It includes:

- Custom JWT token generation and validation services
- Refresh token implementation with secure storage
- Multiple JWT issuer support (app tokens + dotnet user-jwts)
- Structured API responses using record types
- Comprehensive Swagger/OpenAPI documentation with security definitions
- Role-based authorization
- Modern minimal API patterns with `MapGroup` and `TypedResults`
- Security headers and error handling middleware

## Features

### JWT Authentication & Authorization
- **JWT Bearer Authentication**: Industry-standard token-based authentication
- **Access Token Generation**: Short-lived tokens with configurable expiry
- **Refresh Token System**: Secure token renewal without re-authentication
- **Role-Based Authorization**: Admin, Manager, and User roles
- **Multiple Token Issuers**: Support for both application and development tokens
- **Secure Token Validation**: Comprehensive validation with proper error handling

### API Endpoints
- **Public Endpoints**: Health checks and public weather data
- **Authentication Endpoints**: Login, logout, and token refresh
- **Protected Endpoints**: User-specific weather data and profile information
- **Admin Endpoints**: Administrative functions requiring elevated privileges

### Modern ASP.NET Core Patterns
- **Minimal APIs**: Clean endpoint definitions with `MapGroup`
- **TypedResults**: Strongly-typed HTTP responses
- **Record Types**: Immutable request/response DTOs
- **Dependency Injection**: Scoped services and proper DI patterns
- **Problem Details**: Standardized error responses (RFC 7807)
- **Security Headers**: OWASP-recommended security headers
- **Policy Schemes**: Multiple authentication schemes with dynamic selection

### Development Tools Integration
- **dotnet user-jwts**: Integration with .NET development JWT tool
- **Swagger UI**: Interactive API documentation and testing
- **Development Tokens**: Pre-configured test tokens for development
- **Comprehensive Logging**: Structured logging for authentication events

## Getting Started

### Prerequisites
- .NET 9 SDK or later
- Visual Studio 2022, Visual Studio Code, or JetBrains Rider

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd AuthNAuthZPlayground/JwtAuthentication
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access the API**
   - Swagger UI: `https://localhost:7158`
   - API Base URL: `https://localhost:7158/api`

### Test Users

The application includes pre-configured test users:

| Username | Password    | Roles            | Description          |
|----------|-------------|------------------|----------------------|
| admin    | admin123    | Admin, User      | Administrator        |
| user     | user123     | User             | Regular User         |
| test     | test123     | User             | Test User            |
| demo     | demo123     | User             | Demo User            |
| manager  | manager123  | Manager, User    | Department Manager   |

## Project Structure

```
JwtAuthentication/
├── Authentication/                          # (Future: Custom auth handlers)
├── Models/
│   ├── AuthenticationModels.cs             # JWT-specific request/response models
│   └── ApiModels.cs                        # General API models
├── Services/
│   ├── IAuthenticationServices.cs          # Service interfaces
│   ├── UserService.cs                      # User validation and claims
│   ├── JwtTokenService.cs                  # JWT generation and validation
│   └── RefreshTokenService.cs              # Refresh token management
├── Program.cs                              # Application configuration and endpoints
├── JwtAuthentication.csproj                # Project file with JWT dependencies
├── JwtAuthentication.http                  # HTTP requests for testing
└── README.md                              # This file
```

## Key Components

### JWT Token Service
Handles JWT token operations:
- **Token Generation**: Creates signed JWTs with user claims
- **Token Validation**: Validates incoming tokens with comprehensive checks
- **Security Configuration**: Proper signing key management and validation parameters

### Refresh Token Service
Manages refresh token lifecycle:
- **Token Storage**: In-memory storage for demo (replaceable with database)
- **Token Validation**: Secure refresh token verification
- **Token Rotation**: Automatic refresh token rotation on use
- **Cleanup**: Automatic removal of expired tokens

### User Service
Provides user authentication:
- **Credential Validation**: Secure username/password verification
- **Claims Generation**: Creates JWT claims from user data
- **Timing Attack Prevention**: Consistent validation time regardless of user existence

### Multiple Authentication Schemes
Supports different token types:
- **Application JWT**: Tokens issued by the application login endpoint
- **Development JWT**: Tokens created by `dotnet user-jwts` tool
- **Policy-Based Selection**: Automatic scheme selection based on token properties

## Configuration

### JWT Settings
Configure JWT behavior in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "Your-256-bit-secret-key-here",
    "Issuer": "https://your-domain.com",
    "Audience": "https://your-domain.com",
    "ExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Authentication Configuration
```csharp
// Multiple JWT schemes with policy selection
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("JWT", options => { /* App JWT config */ })
    .AddJwtBearer("DevJWT", options => { /* Dev JWT config */ })
    .AddPolicyScheme("Bearer", "Bearer", options => {
        // Dynamic scheme selection logic
    });
```

### Authorization Policies
```csharp
// Role-based authorization
.RequireAuthorization(policy => policy.RequireRole("Admin"))

// Custom policies (can be extended)
.RequireAuthorization("CustomPolicy")
```

## API Reference

### Authentication Endpoints

#### POST /api/auth/login
Authenticates user and returns JWT tokens.

**Request:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-refresh-token",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "user": {
    "username": "admin",
    "displayName": "Administrator",
    "email": "admin@example.com",
    "roles": ["Admin", "User"]
  }
}
```

#### POST /api/auth/refresh
Exchanges refresh token for new access token.

**Request:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new-base64-encoded-refresh-token",
  "tokenType": "Bearer",
  "expiresIn": 900
}
```

#### POST /api/auth/logout
Revokes the refresh token (requires authentication).

**Request:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

### Protected Endpoints

#### GET /api/protected/weather
Returns extended weather forecast (requires authentication).

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

#### GET /api/protected/user-info
Returns current user information (requires authentication).

#### GET /api/admin/users
Returns all users (requires Admin role).

## Development with dotnet user-jwts

The project supports the `dotnet user-jwts` tool for development testing:

### Create Development Tokens

```bash
# Create a user token
dotnet user-jwts create --name testuser --role User

# Create an admin token
dotnet user-jwts create --name testadmin --role Admin --role User

# Create a token with custom claims
dotnet user-jwts create --name customuser --role User --claim "department=Engineering"
```

### List Development Tokens

```bash
dotnet user-jwts list
```

### Remove Development Tokens

```bash
dotnet user-jwts remove {token-id}
```

### Testing with Development Tokens

Use the generated tokens in HTTP requests:

```http
GET https://localhost:7158/api/protected/weather
Authorization: Bearer {dotnet-user-jwt-token}
```

## Security Best Practices Implemented

### Token Security
- **Short-lived Access Tokens**: Default 15-minute expiry
- **Secure Refresh Tokens**: Cryptographically random, longer expiry
- **Token Rotation**: Refresh tokens are replaced on use
- **Proper Validation**: Comprehensive JWT validation parameters

### Authentication Security
- **Timing Attack Prevention**: Consistent validation time
- **Secure String Comparison**: Constant-time password comparison
- **Comprehensive Logging**: Security events are logged appropriately
- **Error Handling**: Standardized error responses without information leakage

### API Security
- **HTTPS Enforcement**: Automatic HTTPS redirection
- **Security Headers**: OWASP-recommended headers
- **Input Validation**: Proper request validation
- **Authorization Checks**: Role-based and policy-based authorization

### Development Security
- **Configuration Separation**: Different keys for development/production
- **Development Token Support**: Safe development testing with dotnet user-jwts
- **Secret Management**: Proper configuration of sensitive values

## Production Considerations

### Security Enhancements
1. **Key Management**: Use Azure Key Vault or similar for JWT signing keys
2. **Certificate-Based Signing**: Consider RSA/ECDSA certificates instead of symmetric keys
3. **Token Blacklisting**: Implement token revocation lists for compromised tokens
4. **Rate Limiting**: Add rate limiting to authentication endpoints
5. **Audit Logging**: Comprehensive security event logging

### Database Integration
Replace in-memory storage with:
- **Entity Framework Core**: For relational database storage
- **Redis**: For distributed refresh token caching
- **Azure Cosmos DB**: For NoSQL storage scenarios
- **Identity Providers**: Integration with Azure AD, Auth0, IdentityServer

### Performance Optimizations
- **Token Caching**: Cache validated tokens to reduce validation overhead
- **Connection Pooling**: Optimize database connections
- **Async Patterns**: Ensure all I/O operations are async
- **Distributed Caching**: Use Redis for multi-instance deployments

### Monitoring and Observability
- **Application Insights**: Telemetry and performance monitoring
- **Health Checks**: Comprehensive health monitoring
- **Metrics**: Authentication success/failure rates
- **Alerts**: Security event alerting

## Dependencies

- **Microsoft.AspNetCore.Authentication.JwtBearer** (9.0.8): JWT Bearer authentication
- **Microsoft.IdentityModel.Tokens** (8.1.2): Token validation and signing
- **System.IdentityModel.Tokens.Jwt** (8.1.2): JWT token handling
- **Microsoft.AspNetCore.OpenApi** (9.0.8): OpenAPI support
- **Swashbuckle.AspNetCore** (7.2.0): Swagger UI and documentation
- **.NET Aspire ServiceDefaults**: Telemetry and service discovery

## Testing

### Manual Testing
Use the included `JwtAuthentication.http` file with:
- Visual Studio 2022 (built-in support)
- Visual Studio Code with REST Client extension
- JetBrains Rider (built-in support)

### Automated Testing
The project structure supports:
- **Unit Tests**: Service layer testing
- **Integration Tests**: Full API endpoint testing
- **Security Tests**: Authentication and authorization testing

### Load Testing
Consider testing with:
- **NBomber**: .NET load testing framework
- **k6**: Modern load testing tool
- **Apache Bench**: Simple HTTP benchmarking

## Troubleshooting

### Common Issues

#### Token Validation Errors
- Check JWT configuration matches between issuer and audience
- Verify signing key is correct and properly encoded
- Ensure system clock is synchronized (clock skew issues)

#### Authentication Failures
- Verify user credentials are correct
- Check user service is properly registered in DI container
- Review authentication event logs

#### Authorization Issues
- Confirm user has required roles/claims
- Verify authorization policies are correctly configured
- Check if custom authorization handlers are working

## Contributing

1. Follow the existing code style and patterns
2. Add unit tests for new functionality
3. Update documentation for any API changes
4. Ensure all security best practices are maintained
5. Test with both application and development JWT tokens

## License

This is a demonstration project for educational purposes.

## Related Resources

- [ASP.NET Core JWT Authentication Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [JWT Bearer Authentication Configuration](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [Policy Schemes in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/policyschemes)
- [JSON Web Token (JWT) RFC 7519](https://datatracker.ietf.org/doc/html/rfc7519)
- [OAuth 2.0 Authorization Framework](https://datatracker.ietf.org/doc/html/rfc6749)
- [OWASP Authentication Guidelines](https://owasp.org/www-project-cheat-sheets/cheatsheets/Authentication_Cheat_Sheet.html)
- [Minimal APIs in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)