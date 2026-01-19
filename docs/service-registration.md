# Service Registration

WebServiceToolkit provides attribute-based automatic service registration for dependency injection.

## Overview

Instead of manually registering each service in `Program.cs`, you can mark your service classes with attributes and let the toolkit register them automatically.

## Attributes

### [WebService]

Marks a class for automatic registration as a scoped service:

```csharp
using DevInstance.WebServiceToolkit.Tools;

public interface IProductService
{
    Task<Product> GetByIdAsync(string id);
}

[WebService]
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product> GetByIdAsync(string id)
    {
        return await _repository.FindByIdAsync(id);
    }
}
```

### [WebServiceMock]

Marks a class as a mock implementation for testing:

```csharp
[WebServiceMock]
public class MockProductService : IProductService
{
    private readonly List<Product> _products = new()
    {
        new Product { Id = "1", Name = "Test Product", Price = 9.99m }
    };

    public Task<Product> GetByIdAsync(string id)
    {
        return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    }
}
```

## Registration

### Production Services

Register all `[WebService]` classes from the calling assembly:

```csharp
using DevInstance.WebServiceToolkit.Tools;

var builder = WebApplication.CreateBuilder(args);

// Auto-register all [WebService] classes
builder.Services.AddServerWebServices();
```

### From a Specific Assembly

Register services from a specific assembly:

```csharp
using System.Reflection;

// From current assembly
builder.Services.AddServerWebServices(Assembly.GetExecutingAssembly());

// From another assembly
builder.Services.AddServerWebServices(typeof(ProductService).Assembly);
```

### Mock Services

Register mock implementations for testing:

```csharp
// In test setup or development environment
builder.Services.AddServerWebServicesMocks();

// Or from specific assembly
builder.Services.AddServerWebServicesMocks(typeof(MockProductService).Assembly);
```

## How It Works

The registration process:

1. Scans the assembly for classes with `[WebService]` or `[WebServiceMock]` attribute
2. For each class, finds all implemented interfaces
3. Registers the class as a **scoped** service for each interface
4. If no interfaces are implemented, registers the class itself

```csharp
// Given this service:
[WebService]
public class ProductService : IProductService, IProductAdmin
{
    // ...
}

// Equivalent manual registration:
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductAdmin, ProductService>();
```

## Environment-Based Registration

Use different implementations for different environments:

```csharp
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Use mock services in development
    builder.Services.AddServerWebServicesMocks();
}
else
{
    // Use real services in production
    builder.Services.AddServerWebServices();
}
```

## Multiple Assemblies

Register services from multiple assemblies:

```csharp
builder.Services.AddServerWebServices(typeof(Program).Assembly);
builder.Services.AddServerWebServices(typeof(ExternalService).Assembly);
```

## Best Practices

### 1. Define Interfaces

Always define interfaces for your services:

```csharp
// Good: Interface defined
public interface IProductService
{
    Task<Product> GetByIdAsync(string id);
}

[WebService]
public class ProductService : IProductService { }

// Avoid: No interface
[WebService]
public class ProductService { }
```

### 2. One Implementation Per Interface

Avoid registering multiple implementations of the same interface:

```csharp
// Be careful: Both will register for IProductService
[WebService]
public class ProductService : IProductService { }

[WebService]
public class ProductServiceV2 : IProductService { }  // Will override

// Solution: Use different interfaces or manual registration
public interface IProductServiceV2 : IProductService { }

[WebService]
public class ProductServiceV2 : IProductServiceV2 { }
```

### 3. Organize by Feature

Group related services:

```
src/
├── Products/
│   ├── IProductService.cs
│   ├── ProductService.cs        [WebService]
│   └── MockProductService.cs    [WebServiceMock]
├── Orders/
│   ├── IOrderService.cs
│   ├── OrderService.cs          [WebService]
│   └── MockOrderService.cs      [WebServiceMock]
```

### 4. Use Mocks for Integration Tests

```csharp
public class ProductsControllerTests
{
    [Fact]
    public async Task GetProduct_ReturnsProduct()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddServerWebServicesMocks();  // Use mocks

        var app = builder.Build();
        var client = app.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/1");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

## Combining with Manual Registration

You can combine automatic and manual registration:

```csharp
// Auto-register most services
builder.Services.AddServerWebServices();

// Manually register services with special configuration
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>();
```

## Scoped Lifetime

All services registered via `AddServerWebServices` use **scoped** lifetime:

- A new instance is created per HTTP request
- The instance is shared within the same request
- The instance is disposed at the end of the request

If you need a different lifetime (singleton or transient), register the service manually:

```csharp
// Singleton
builder.Services.AddSingleton<IConfigService, ConfigService>();

// Transient
builder.Services.AddTransient<IGuidGenerator, GuidGenerator>();
```

## See Also

- [Getting Started](getting-started.md)
- [Exception Handling](exception-handling.md)
