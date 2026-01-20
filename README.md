# WebServiceToolkit

WebServiceToolkit is a .NET library designed to simplify the development of ASP.NET Core web services. It provides utilities for query model binding, standardized exception handling, dependency injection patterns, and paginated response models.

## Features

- **Query Model Binding** - Automatically bind query parameters to strongly-typed POCO classes
- **Exception Handling** - Standardized HTTP exception types with automatic response mapping
- **Service Registration** - Attribute-based automatic dependency injection registration
- **Pagination Support** - Built-in models for paginated, sortable, and searchable responses
- **Database Query Interfaces** - Fluent interfaces for building database queries

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| [DevInstance.WebServiceToolkit](https://www.nuget.org/packages/DevInstance.WebServiceToolkit) | 10.0.1 | Main package with ASP.NET Core integration |
| [DevInstance.WebServiceToolkit.Common](https://www.nuget.org/packages/DevInstance.WebServiceToolkit.Common) | 10.0.1 | Common models and attributes (no ASP.NET Core dependency) |
| [DevInstance.WebServiceToolkit.Database](https://www.nuget.org/packages/DevInstance.WebServiceToolkit.Database) | 10.0.1 | Database query interfaces |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Installation

Install the main package (includes Common and Database as dependencies):

```bash
dotnet add package DevInstance.WebServiceToolkit
```

Or install individual packages:

```bash
dotnet add package DevInstance.WebServiceToolkit.Common
dotnet add package DevInstance.WebServiceToolkit.Database
```

## Quick Start

### 1. Register Services

In your `Program.cs`:

```csharp
using DevInstance.WebServiceToolkit.Http.Query;
using DevInstance.WebServiceToolkit.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add query model binding support
builder.Services.AddControllers()
    .AddWebServiceToolkitQuery();

// Auto-register services marked with [WebService] attribute
builder.Services.AddServerWebServices();
```

### 2. Create a Query Model

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

    public bool IsAscending { get; set; } = true;
}
```

### 3. Use in a Controller

```csharp
using DevInstance.WebServiceToolkit.Common.Model;
using DevInstance.WebServiceToolkit.Controllers;
using DevInstance.WebServiceToolkit.Exceptions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public Task<ActionResult<ModelList<Product>>> GetProducts(ProductQuery query)
    {
        return this.HandleWebRequestAsync<ModelList<Product>>(async () =>
        {
            var products = await _productService.GetProductsAsync(query);
            return Ok(products);
        });
    }

    [HttpGet("{id}")]
    public Task<ActionResult<Product>> GetProduct(string id)
    {
        return this.HandleWebRequestAsync<Product>(async () =>
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                throw new RecordNotFoundException(id);
            return Ok(product);
        });
    }
}
```

### 4. Create a Service

```csharp
using DevInstance.WebServiceToolkit.Tools;

public interface IProductService
{
    Task<ModelList<Product>> GetProductsAsync(ProductQuery query);
    Task<Product?> GetByIdAsync(string id);
}

[WebService]
public class ProductService : IProductService
{
    // Implementation auto-registered via AddServerWebServices()
}
```

## Documentation

- [Getting Started](docs/getting-started.md) - Installation and setup guide
- [Query Model Binding](docs/query-model-binding.md) - Using `[QueryModel]` and `[QueryName]` attributes
- [Exception Handling](docs/exception-handling.md) - HTTP exception types and `ControllerUtils`
- [Service Registration](docs/service-registration.md) - `[WebService]` and `[WebServiceMock]` attributes
- [Database Queries](docs/database-queries.md) - `IModelQuery` and related interfaces
- [Pagination](docs/pagination.md) - `ModelList<T>` for paginated responses

## API Reference

### Common Package Types

| Type | Description |
|------|-------------|
| `ModelItem` | Base class with `Id` property for server-assigned identifiers |
| `ModelList<T>` | Paginated, sortable, searchable collection response DTO |
| `ModelListResult` | Utility for creating single-item list responses |
| `QueryModelAttribute` | Marks a class for automatic query parameter binding |
| `QueryNameAttribute` | Overrides the query parameter name for a property |

### Main Package Types

| Type | Description |
|------|-------------|
| `QueryModelBinder` | Core query string parsing engine |
| `QueryModelBindException` | Exception with per-field validation errors |
| `RegistrationExtensions` | DI registration methods for query model binding |
| `BadRequestException` | Throws HTTP 400 Bad Request |
| `RecordNotFoundException` | Throws HTTP 404 Not Found |
| `RecordConflictException` | Throws HTTP 409 Conflict |
| `UnauthorizedException` | Throws HTTP 401 Unauthorized |
| `ControllerUtils` | Exception handling wrapper for controller actions |
| `WebServiceAttribute` | Marks class for automatic DI registration |
| `WebServiceMockAttribute` | Marks mock implementation for testing scenarios |
| `ServiceConfigurationExtensions` | Assembly scanning for attributed services |

### Database Package Types

| Type | Description |
|------|-------------|
| `IModelQuery<T,D>` | Base query interface with CRUD operations |
| `IQPageable<T>` | Skip/Take pagination interface |
| `IQSearchable<T,K>` | Search and lookup by public ID interface |
| `IQSortable<T>` | Sort by column interface |

## License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests on [GitHub](https://github.com/devInstance/WebServiceToolkit).
