---
description: 'Entity Framework Core Best Practices'
applyTo: '**/*DbContext.cs'
---

# Entity Framework Core Best Practices

## DbContext Configuration

### Connection Management
- Use dependency injection for DbContext
- Configure connection strings in appsettings.json
- Use connection pooling for better performance

### Entity Configuration
```csharp
// ✅ Good - Use Fluent API for complex configurations
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Email)
              .IsRequired()
              .HasMaxLength(255);
        entity.HasIndex(e => e.Email)
              .IsUnique();
    });
    
    base.OnModelCreating(modelBuilder);
}
```

### Query Optimization
- Use `AsNoTracking()` for read-only queries
- Include related data efficiently with `Include()`
- Avoid N+1 queries by using `ThenInclude()`
- Use projection (`Select()`) for minimal data retrieval

### Migration Best Practices
- Keep migrations small and focused
- Review generated migration scripts before applying
- Use descriptive migration names
- Never modify existing migrations in production

### Performance Guidelines
- Enable query logging for development
- Use compiled queries for frequently executed queries
- Implement query splitting for complex includes
- Consider raw SQL for complex queries

### Concurrency Handling
- Implement optimistic concurrency with row versions
- Handle `DbUpdateConcurrencyException` appropriately
- Use transactions for multi-entity operations

### Security Considerations
- Always use parameterized queries
- Validate input data before database operations
- Implement proper authorization at the data layer
- Use database connection encryption
