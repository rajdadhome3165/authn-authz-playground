---
description: 'C# Async Programming Best Practices'
applyTo: '**/*.cs'
---

# C# Async Programming Best Practices

## Async/Await Guidelines

### Core Principles
- Always use `async`/`await` for I/O-bound operations
- Use `Task.Run` only for CPU-bound work that should run on a background thread
- Avoid `async void` except for event handlers
- Use `ConfigureAwait(false)` in library code to avoid deadlocks

### Method Naming
- Suffix async methods with "Async" (e.g., `GetDataAsync()`)
- Return `Task` or `Task<T>` from async methods
- Use `ValueTask<T>` for high-performance scenarios with frequent synchronous completion

### Exception Handling
```csharp
// ✅ Good - Properly handle exceptions in async methods
public async Task<string> GetDataAsync()
{
    try
    {
        return await httpClient.GetStringAsync(url);
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Failed to fetch data from {Url}", url);
        throw;
    }
}
```

### Cancellation Support
- Always accept `CancellationToken` parameters for long-running operations
- Pass cancellation tokens through the call chain
- Check for cancellation at appropriate points

```csharp
public async Task<List<T>> ProcessItemsAsync<T>(
    IEnumerable<T> items, 
    CancellationToken cancellationToken = default)
{
    var results = new List<T>();
    
    foreach (var item in items)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await ProcessItemAsync(item, cancellationToken);
        results.Add(result);
    }
    
    return results;
}
```

### Performance Considerations
- Use `Task.WhenAll()` for concurrent operations
- Avoid blocking async calls with `.Result` or `.Wait()`
- Consider `IAsyncEnumerable<T>` for streaming data

### Common Anti-Patterns to Avoid
- `async void` methods (except event handlers)
- Mixing synchronous and asynchronous code
- Not using cancellation tokens
- Creating unnecessary tasks with `Task.Run` for already async operations
