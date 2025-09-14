---
description: 'Guidelines for building C# applications with modern best practices'
applyTo: '**/*.cs'
---

# C# Development Guidelines

## C# Language Features
- Always use the latest version C#, currently C# 13 features
- Leverage pattern matching, nullable reference types, and modern language constructs
- Use file-scoped namespace declarations and global using statements
- Prefer record types for immutable data models
- Use primary constructors where appropriate

## Code Quality Standards
- Write clear and concise comments for complex logic and design decisions
- Handle edge cases and implement comprehensive exception handling
- For external dependencies, document their usage and purpose in comments
- Follow SOLID principles and clean code practices
- Implement proper logging using structured logging (Serilog, NLog, etc.)

## Naming Conventions
- Follow PascalCase for component names, method names, and public members
- Use camelCase for private fields and local variables
- Prefix interface names with "I" (e.g., IUserService)
- Use meaningful names that express intent
- Avoid abbreviations and single-letter variables (except loop counters)

## Formatting and Style
- Apply code-formatting style defined in `.editorconfig`
- Prefer file-scoped namespace declarations
- Insert a newline before the opening curly brace of any code block
- Ensure that the final return statement of a method is on its own line
- Use pattern matching and switch expressions wherever possible
- Use `nameof` instead of string literals when referring to member names
- Ensure XML doc comments are created for all public APIs with `<example>` tags when applicable

## Modern .NET Practices
- Use dependency injection and the built-in IoC container
- Implement configuration using the Options pattern
- Use minimal APIs for simple HTTP endpoints
- Leverage middleware for cross-cutting concerns
- Implement health checks for production applications

## Security Best Practices
- Always validate input data and sanitize outputs
- Use parameterized queries to prevent SQL injection
- Implement proper authentication and authorization
- Follow OWASP security guidelines
- Use secure communication protocols (HTTPS, secure connections)

## Performance Optimization
- Use async/await for I/O-bound operations
- Implement caching strategies where appropriate
- Use memory-efficient data structures and algorithms
- Profile and optimize critical code paths
- Consider using `Span<T>` and `Memory<T>` for high-performance scenarios

## Project Setup and Structure

- Guide users through creating a new .NET project with the appropriate templates.
- Explain the purpose of each generated file and folder to build understanding of the project structure.
- Demonstrate how to organize code using feature folders or domain-driven design principles.
- Show proper separation of concerns with models, services, and data access layers.
- Explain the Program.cs and configuration system in ASP.NET Core 9 including environment-specific settings.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## Data Access Patterns

- Guide the implementation of a data access layer using Entity Framework Core.
- Explain different options (SQL Server, SQLite, In-Memory) for development and production.
- Demonstrate repository pattern implementation and when it's beneficial.
- Show how to implement database migrations and data seeding.
- Explain efficient query patterns to avoid common performance issues.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.

## Validation and Error Handling

- Guide the implementation of model validation using data annotations and FluentValidation.
- Explain the validation pipeline and how to customize validation responses.
- Demonstrate a global exception handling strategy using middleware.
- Show how to create consistent error responses across the API.
- Explain problem details (RFC 7807) implementation for standardized error responses.

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Explain integration testing approaches for API endpoints.
- Demonstrate how to mock dependencies for effective testing.
- Show how to test authentication and authorization logic.
- Explain test-driven development principles as applied to API development.

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.

## Deployment and DevOps

- Guide users through containerizing their API using .NET's built-in container support (`dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`).
- Explain the differences between manual Dockerfile creation and .NET's container publishing features.
- Explain CI/CD pipelines for NET applications.
- Demonstrate deployment to Azure App Service, Azure Container Apps, or other hosting options.
- Show how to implement health checks and readiness probes.
- Explain environment-specific configurations for different deployment stages.