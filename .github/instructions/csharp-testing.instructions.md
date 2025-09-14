---
description: 'C# Testing Best Practices'
applyTo: '**/*Test.cs'
---

# C# Testing Best Practices

## Testing Framework Guidelines

### Test Structure
- Follow the Arrange-Act-Assert (AAA) pattern
- One assertion per test method when possible
- Use descriptive test method names that explain the scenario

### Naming Conventions
```csharp
// ✅ Good - Descriptive test names
[Test]
public void CalculateTotal_WithValidItems_ReturnsCorrectSum()
{
    // Arrange
    var calculator = new Calculator();
    var items = new[] { 1.5m, 2.3m, 4.2m };
    
    // Act
    var result = calculator.CalculateTotal(items);
    
    // Assert
    result.Should().Be(8.0m);
}
```

### Test Data Management
- Use test builders or factory methods for complex object creation
- Leverage data-driven tests for multiple scenarios
- Keep test data focused and minimal

### Mocking Guidelines
- Mock external dependencies and infrastructure concerns
- Don't mock value objects or simple data containers
- Use meaningful mock setups that reflect real behavior

### Integration Tests
- Test against real databases using test containers when possible
- Use separate test configuration and connection strings
- Clean up test data after each test run

### Performance Testing
- Include performance tests for critical paths
- Set realistic performance thresholds
- Use BenchmarkDotNet for micro-benchmarks

## Test Categories
- **Unit Tests**: Fast, isolated, test single components
- **Integration Tests**: Test component interactions
- **End-to-End Tests**: Test complete user scenarios
- **Contract Tests**: Verify API contracts and behavior
