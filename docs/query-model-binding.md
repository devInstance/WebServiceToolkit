# Query Model Binding

WebServiceToolkit provides automatic binding of HTTP query string parameters to strongly-typed POCO classes.

## Overview

Instead of manually parsing query parameters, you can define a class with the `[QueryModel]` attribute and let the framework automatically populate it from the request.

## Basic Usage

### 1. Define a Query Model

```csharp
using DevInstance.WebServiceToolkit.Http.Query;
using System.ComponentModel;

[QueryModel]
public class ProductQuery
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
}
```

### 2. Use in Controller

```csharp
[HttpGet]
public ActionResult<ModelList<Product>> GetProducts(ProductQuery query)
{
    // query.Page, query.PageSize, query.Search are automatically populated
    // from ?page=0&pageSize=20&search=widget
}
```

## Supported Types

Query model binding supports the following property types:

| Type | Example Query String |
|------|---------------------|
| `string` | `?name=widget` |
| `bool` | `?active=true` |
| `int` | `?page=1` |
| `long` | `?id=12345678901` |
| `decimal` | `?price=19.99` |
| `double` | `?rate=3.14159` |
| `Guid` | `?id=550e8400-e29b-41d4-a716-446655440000` |
| `DateTime` | `?date=2024-01-15T10:30:00Z` |
| `DateOnly` (.NET 7+) | `?date=2024-01-15` |
| `TimeOnly` (.NET 7+) | `?time=10:30:00` or `?time=10:30` |
| Enums | `?status=Active` |
| Nullable<T> | Any above with `?` modifier |
| Arrays/Collections | `?ids=1,2,3` (comma-separated) |

## Default Values

Use `[DefaultValue]` to specify defaults when a parameter is not provided:

```csharp
[QueryModel]
public class ProductQuery
{
    [DefaultValue(0)]
    public int Page { get; set; }

    [DefaultValue(20)]
    public int PageSize { get; set; }

    [DefaultValue(true)]
    public bool IncludeInactive { get; set; }

    [DefaultValue(ProductStatus.Active)]
    public ProductStatus Status { get; set; }
}
```

## Custom Parameter Names

Use `[QueryName]` to map a property to a different query parameter name:

```csharp
[QueryModel]
public class SearchQuery
{
    [QueryName("q")]
    public string SearchText { get; set; }

    [QueryName("page_size")]
    public int PageSize { get; set; }

    [QueryName("include_deleted")]
    public bool IncludeDeleted { get; set; }
}
```

This maps:
- `?q=hello` to `SearchText`
- `?page_size=20` to `PageSize`
- `?include_deleted=true` to `IncludeDeleted`

## Arrays and Collections

Properties that are arrays or implement `IEnumerable<T>` are bound from comma-separated values:

```csharp
[QueryModel]
public class ProductFilter
{
    public string[] Categories { get; set; }  // ?categories=electronics,books,toys
    public List<int> Ids { get; set; }        // ?ids=1,2,3,4,5
    public ProductStatus[] Statuses { get; set; } // ?statuses=Active,Pending
}
```

## Enums

Enums are parsed case-insensitively:

```csharp
public enum ProductStatus
{
    Draft,
    Active,
    Discontinued
}

[QueryModel]
public class ProductQuery
{
    public ProductStatus? Status { get; set; }  // ?status=active or ?status=Active
}
```

## Registration

### Option 1: Chain from AddControllers (recommended)

```csharp
builder.Services.AddControllers()
    .AddWebServiceToolkitQuery();
```

### Option 2: Separate registration

```csharp
builder.Services.AddControllers();
builder.Services.AddWebServiceToolkitQuery();
```

### Option 3: Manual MvcOptions configuration

```csharp
builder.Services.AddControllers(options =>
{
    options.UseWebServiceToolkitQuery();
});
```

## Error Handling

When binding fails, errors are added to ModelState. You can check for validation errors:

```csharp
[HttpGet]
public ActionResult<ModelList<Product>> GetProducts(ProductQuery query)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    // ...
}
```

## Manual Binding

You can also bind manually without MVC integration:

```csharp
// Throws QueryModelBindException on failure
var query = QueryModelBinder.Bind<ProductQuery>(HttpContext.Request);

// Or use TryBind for error handling
if (QueryModelBinder.TryBind<ProductQuery>(request, out var query, out var errors))
{
    // Use query
}
else
{
    // Handle errors dictionary
    foreach (var error in errors)
    {
        Console.WriteLine($"{error.Key}: {error.Value}");
    }
}
```

## Best Practices

1. **Use nullable types** for optional parameters:
   ```csharp
   public string? Search { get; set; }  // Optional
   public int Page { get; set; }        // Required (default 0)
   ```

2. **Set sensible defaults** for pagination:
   ```csharp
   [DefaultValue(0)]
   public int Page { get; set; }

   [DefaultValue(20)]
   public int PageSize { get; set; }
   ```

3. **Use enums** for fixed option sets:
   ```csharp
   public SortDirection? Direction { get; set; }
   ```

4. **Use `[QueryName]`** for API consistency when C# naming differs:
   ```csharp
   [QueryName("sort_by")]
   public string SortBy { get; set; }
   ```

## See Also

- [Getting Started](getting-started.md)
- [Pagination](pagination.md)
