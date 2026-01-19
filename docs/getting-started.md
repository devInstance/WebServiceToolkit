# Getting Started with WebServiceToolkit

This guide walks you through setting up WebServiceToolkit in an ASP.NET Core project.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- An ASP.NET Core Web API project

## Installation

### Option 1: Install the main package (recommended)

The main package includes all dependencies:

```bash
dotnet add package DevInstance.WebServiceToolkit
```

### Option 2: Install individual packages

If you only need specific functionality:

```bash
# Common models only (no ASP.NET Core dependency)
dotnet add package DevInstance.WebServiceToolkit.Common

# Database query interfaces only
dotnet add package DevInstance.WebServiceToolkit.Database
```

## Basic Setup

### 1. Configure Services

In your `Program.cs`, register the WebServiceToolkit services:

```csharp
using DevInstance.WebServiceToolkit.Http.Query;
using DevInstance.WebServiceToolkit.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add controllers with query model binding
builder.Services.AddControllers()
    .AddWebServiceToolkitQuery();

// Auto-register services marked with [WebService] attribute
builder.Services.AddServerWebServices();

var app = builder.Build();

app.MapControllers();
app.Run();
```

### 2. Create a Model

Create a model class that inherits from `ModelItem`:

```csharp
using DevInstance.WebServiceToolkit.Common.Model;

public class Product : ModelItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}
```

### 3. Create a Query Model

Define a query model for filtering/pagination:

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

    [DefaultValue(true)]
    public bool IsAscending { get; set; }

    public string? Category { get; set; }
}
```

### 4. Create a Service

Create a service interface and implementation:

```csharp
using DevInstance.WebServiceToolkit.Common.Model;
using DevInstance.WebServiceToolkit.Tools;

public interface IProductService
{
    Task<ModelList<Product>> GetProductsAsync(ProductQuery query);
    Task<Product?> GetByIdAsync(string id);
    Task<Product> CreateAsync(CreateProductRequest request);
    Task<Product> UpdateAsync(string id, UpdateProductRequest request);
    Task DeleteAsync(string id);
}

[WebService]
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ModelList<Product>> GetProductsAsync(ProductQuery query)
    {
        var products = await _repository.QueryAsync(query);
        var totalCount = await _repository.CountAsync(query);

        return new ModelList<Product>
        {
            Items = products.ToArray(),
            TotalCount = totalCount,
            PagesCount = (int)Math.Ceiling(totalCount / (double)query.PageSize),
            Page = query.Page,
            Count = products.Count,
            SortBy = query.SortBy,
            IsAsc = query.IsAscending,
            Search = query.Search
        };
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _repository.FindByIdAsync(id);
    }

    // ... other methods
}
```

### 5. Create a Controller

Create a controller that uses the service:

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

    [HttpPost]
    public Task<ActionResult<Product>> CreateProduct(CreateProductRequest request)
    {
        return this.HandleWebRequestAsync<Product>(async () =>
        {
            var product = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        });
    }

    [HttpPut("{id}")]
    public Task<ActionResult<Product>> UpdateProduct(string id, UpdateProductRequest request)
    {
        return this.HandleWebRequestAsync<Product>(async () =>
        {
            var product = await _productService.UpdateAsync(id, request);
            return Ok(product);
        });
    }

    [HttpDelete("{id}")]
    public Task<ActionResult<bool>> DeleteProduct(string id)
    {
        return this.HandleWebRequestAsync<bool>(async () =>
        {
            await _productService.DeleteAsync(id);
            return Ok(true);
        });
    }
}
```

## Testing the API

Once your application is running, you can test the endpoints:

```bash
# Get all products with pagination
curl "http://localhost:5000/api/products?page=0&pageSize=10"

# Search for products
curl "http://localhost:5000/api/products?search=widget&sort=name"

# Get a specific product
curl "http://localhost:5000/api/products/abc123"

# Create a product
curl -X POST "http://localhost:5000/api/products" \
  -H "Content-Type: application/json" \
  -d '{"name":"Widget","price":9.99}'
```

## Next Steps

- [Query Model Binding](query-model-binding.md) - Learn more about query parameter binding
- [Exception Handling](exception-handling.md) - Understand exception-to-HTTP mapping
- [Service Registration](service-registration.md) - Auto-register services with DI
- [Database Queries](database-queries.md) - Build fluent query objects
- [Pagination](pagination.md) - Implement paginated responses
