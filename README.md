# Authentication & Authorization Playground

A comprehensive .NET 9 demonstration project showcasing various authentication and authorization patterns in ASP.NET Core using .NET Aspire for orchestration.

## 🎯 Project Overview

This repository demonstrates modern authentication and authorization techniques in ASP.NET Core, providing practical examples and best practices for securing web APIs. Each authentication method is implemented as a separate project to show clear separation of concerns and different approaches.

## 🏗️ Project Structure

```
AuthNAuthZPlayground/
├── AuthNAuthZPlayground.AppHost/          # .NET Aspire orchestration
├── AuthNAuthZPlayground.ServiceDefaults/  # Shared service configurations
├── BasicAuthentication/                   # HTTP Basic Authentication implementation
└── [Future authentication types will be added here]
```

## 🔐 Authentication Types

### ✅ Basic Authentication
**Project:** `BasicAuthentication/`

Implements HTTP Basic Authentication with in-memory user storage for demonstration purposes.

**Features:**
- Custom Basic Authentication handler
- Role-based authorization (Admin, User)
- Secure credential validation with timing attack prevention
- Comprehensive API endpoints (public, protected, admin-only)
- OpenAPI/Swagger integration with security definitions
- Built-in test users for demonstration

**Test Users:**
- `admin:admin123` (Admin, User roles)
- `user:user123` (User role)
- `test:test123` (User role)
- `demo:demo123` (User role)

**Endpoints:**
- `GET /api/public/health` - Public health check
- `GET /api/public/weather` - Public weather forecast
- `GET /api/protected/weather` - Protected weather forecast (requires authentication)
- `GET /api/protected/user-info` - User information (requires authentication)
- `GET /api/admin/users` - All users list (requires Admin role)

### 🔄 Planned Authentication Types

The following authentication methods will be added to demonstrate various approaches:

- **JWT Bearer Token Authentication** - Stateless token-based authentication
- **Cookie Authentication** - Session-based authentication with cookies
- **OAuth 2.0 / OpenID Connect** - Third-party authentication (Google, Microsoft, etc.)
- **API Key Authentication** - Simple API key-based authentication
- **Certificate Authentication** - Client certificate-based authentication
- **Multi-Factor Authentication (MFA)** - TOTP/SMS-based additional security
- **Custom Token Authentication** - Custom token schemes

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Aspire dashboard)

### Running the Projects

#### Option 1: Using .NET Aspire (Recommended)

1. **Start the Aspire AppHost:**
   ```bash
   cd AuthNAuthZPlayground.AppHost
   dotnet run
   ```

2. **Access the Aspire Dashboard:**
   - Open your browser to `http://localhost:15888` (URL will be displayed in console)
   - Monitor all services from the centralized dashboard

#### Option 2: Running Individual Projects

1. **Basic Authentication Demo:**
   ```bash
   cd BasicAuthentication
   dotnet run
   ```
   - API: `https://localhost:7082` or `http://localhost:5082`
   - Swagger UI: Available at the root URL

### Testing the APIs

Each project includes a `.http` file with pre-configured requests for testing:

- **BasicAuthentication.http** - Contains all endpoint examples with proper authentication headers

Use these files with:
- Visual Studio 2022 (built-in support)
- Visual Studio Code with REST Client extension
- JetBrains Rider (built-in support)

## 📚 Learning Resources

### Security Best Practices Demonstrated

1. **Secure Authentication Handling**
   - Proper credential validation
   - Timing attack prevention
   - Secure password comparison

2. **Authorization Patterns**
   - Role-based access control (RBAC)
   - Policy-based authorization
   - Endpoint-specific security requirements

3. **API Security**
   - Security headers middleware
   - HTTPS redirection
   - Proper error handling
   - OpenAPI security documentation

4. **Modern .NET Practices**
   - Minimal APIs
   - Dependency injection
   - Configuration patterns
   - Structured logging
   - Health checks

### Architecture Highlights

- **Clean Architecture** - Separation of concerns with services and handlers
- **Dependency Injection** - Proper IoC container usage
- **Configuration Management** - Environment-specific settings
- **Observability** - Logging and monitoring with .NET Aspire
- **API Documentation** - Comprehensive OpenAPI/Swagger integration

## 🛠️ Development

### Project Standards

- **C# 13** with latest language features
- **File-scoped namespaces** for cleaner code
- **Nullable reference types** for better null safety
- **Minimal APIs** for lightweight endpoints
- **Record types** for DTOs and responses
- **Modern async patterns** with proper cancellation support

### Code Quality

- Comprehensive XML documentation
- Structured logging with Serilog patterns
- Proper exception handling
- Input validation and sanitization
- Security-first approach

## 🤝 Contributing

This is an educational project designed to demonstrate authentication patterns. Feel free to:

1. **Fork the repository**
2. **Add new authentication types** following the established patterns
3. **Improve existing implementations** with better practices
4. **Enhance documentation** and examples
5. **Submit pull requests** with clear descriptions

### Adding New Authentication Types

When adding new authentication methods:

1. Create a new project following the naming convention
2. Implement the authentication handler and required services
3. Add comprehensive endpoint examples
4. Create a corresponding `.http` file for testing
5. Update this README with the new authentication type
6. Add the project to the Aspire AppHost for orchestration

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Additional Resources

- [ASP.NET Core Security Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [OpenAPI/Swagger Documentation](https://swagger.io/docs/)
- [OWASP Security Guidelines](https://owasp.org/www-project-top-ten/)

---

**Happy Learning!** 🎓

*This project is designed for educational purposes to demonstrate various authentication and authorization patterns in modern .NET applications.*