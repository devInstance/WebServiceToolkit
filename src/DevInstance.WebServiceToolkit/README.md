# DevInstance.WebServiceToolkit

ASP.NET Core utilities for web service development including query model binding, exception handling, and service registration.

## Installation

```bash
dotnet add package DevInstance.WebServiceToolkit
```

This package includes `DevInstance.WebServiceToolkit.Common` and `DevInstance.WebServiceToolkit.Database` as dependencies.

## Features

- **Query Model Binding** - Automatic binding of query string parameters to strongly-typed POCOs
- **Exception Handling** - Standardized HTTP exception types with automatic response mapping
- **Service Registration** - Attribute-based automatic dependency injection

## Quick Start

### 1. Configure Services

```csharp
using DevInstance.WebServiceToolkit.Http.Query;
using DevInstance.WebServiceToolkit.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add query model binding support
builder.Services.AddControllers()
    .AddWebServiceToolkitQuery();

// Auto-register services marked with [WebService]
builder.Services.AddServerWebServices();
```

### 2. Query Model Binding

Define query models with the `[QueryModel]` attribute:

```csharp
using DevInstance.WebServiceToolkit.Http.Query;
using System.ComponentModel;

[QueryModel]
public class ProductQuery
{
    [DefaultValue(0)]
    public int Page { get; set; }

    [DefaultValue(20)]
    public int PageSize { get; set; }

    public string? Search { get; set; }

    [QueryName("sort")]
    public string? SortBy { get; set; }
}
```

Use in controllers - binding happens automatically:

```csharp
[HttpGet]
public ActionResult<ModelList<Product>> GetProducts(ProductQuery query)
{
    // query is populated from ?page=0&pageSize=20&search=widget&sort=name
}
```

### 3. Exception Handling

Use `HandleWebRequestAsync` to automatically convert exceptions to HTTP responses:

```csharp
using DevInstance.WebServiceToolkit.Controllers;
using DevInstance.WebServiceToolkit.Exceptions;

[HttpGet("{id}")]
public Task<ActionResult<Product>> GetProduct(string id)
{
    return this.HandleWebRequestAsync<Product>(async () =>
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            throw new RecordNotFoundException(id);  // Returns 404
        return Ok(product);
    });
}
```

**Exception to HTTP Status Mapping:**

| Exception | HTTP Status |
|-----------|-------------|
| `RecordNotFoundException` | 404 Not Found |
| `RecordConflictException` | 409 Conflict |
| `UnauthorizedException` | 401 Unauthorized |
| `BadRequestException` | 400 Bad Request |
| Other exceptions | 500 with Problem Details |

### 4. Service Registration

Mark services for automatic DI registration:

```csharp
using DevInstance.WebServiceToolkit.Tools;

public interface IProductService
{
    Task<Product?> GetByIdAsync(string id);
}

[WebService]
public class ProductService : IProductService
{
    // Automatically registered as scoped service
}

// For testing, use WebServiceMock
[WebServiceMock]
public class MockProductService : IProductService
{
    // Register with AddServerWebServicesMocks()
}
```

Register services:

```csharp
// Production
builder.Services.AddServerWebServices();

// Testing
builder.Services.AddServerWebServicesMocks();
```

## API Reference

### Query Model Binding

| Type | Description |
|------|-------------|
| `QueryModelBinder` | Core query string parsing engine |
| `QueryModelBindException` | Exception with per-field validation errors |
| `QueryModelBinderProvider` | MVC model binder provider |
| `QueryModelMvcBinder` | MVC model binder implementation |
| `RegistrationExtensions` | DI registration extension methods |

### Exceptions

| Type | HTTP Status | Description |
|------|-------------|-------------|
| `BadRequestException` | 400 | Invalid request data |
| `UnauthorizedException` | 401 | Authentication required |
| `RecordNotFoundException` | 404 | Resource not found |
| `RecordConflictException` | 409 | Resource conflict |

### Controllers

| Type | Description |
|------|-------------|
| `ControllerUtils` | Extension methods for exception handling |

### Service Registration

| Type | Description |
|------|-------------|
| `WebServiceAttribute` | Marks a class for automatic DI registration |
| `WebServiceMockAttribute` | Marks a mock implementation for testing |
| `ServiceConfigurationExtensions` | Assembly scanning and registration |

## See Also

- [DevInstance.WebServiceToolkit.Common](https://www.nuget.org/packages/DevInstance.WebServiceToolkit.Common) - Common models (no ASP.NET Core dependency)
- [DevInstance.WebServiceToolkit.Database](https://www.nuget.org/packages/DevInstance.WebServiceToolkit.Database) - Database query interfaces
