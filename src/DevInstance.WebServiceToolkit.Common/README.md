# DevInstance.WebServiceToolkit.Common

Common models and attributes for WebServiceToolkit with no ASP.NET Core dependency.

## Installation

```bash
dotnet add package DevInstance.WebServiceToolkit.Common
```

## Features

This package provides:

- **Model classes** for API responses (`ModelItem`, `ModelList<T>`)
- **Query model attributes** for parameter binding (`[QueryModel]`, `[QueryName]`)
- **Utility methods** for creating model responses

## API Reference

### ModelItem

Base class for entities with a server-assigned unique identifier.

```csharp
public class Product : ModelItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// The Id property is inherited from ModelItem
var product = new Product { Id = "abc123", Name = "Widget", Price = 9.99m };
```

### ModelList&lt;T&gt;

Paginated response wrapper with sorting and search metadata.

```csharp
var response = new ModelList<Product>
{
    Items = products.ToArray(),
    TotalCount = 150,
    PagesCount = 8,
    Page = 0,
    Count = 20,
    SortBy = "Name",
    IsAsc = true,
    Search = "widget"
};
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Items` | `T[]` | Array of items for the current page |
| `TotalCount` | `int` | Total items across all pages |
| `PagesCount` | `int` | Total number of pages |
| `Page` | `int` | Current page index (zero-based) |
| `Count` | `int` | Number of items in current page |
| `SortBy` | `string` | Column name used for sorting |
| `IsAsc` | `bool` | True if ascending sort order |
| `Search` | `string` | Applied search query |

### ModelListResult

Utility class for creating `ModelList<T>` instances.

```csharp
// Create a single-item list response
var product = await _repository.GetByIdAsync(id);
return ModelListResult.SingleItemList(product);
```

### QueryModelAttribute

Marks a class for automatic query string parameter binding.

```csharp
[QueryModel]
public class ProductQuery
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
    public ProductCategory? Category { get; set; }
}
```

**Supported types:**
- Primitives: `string`, `bool`, `int`, `long`, `decimal`, `double`
- Date/time: `DateTime`, `DateOnly`, `TimeOnly`
- Other: `Guid`, enums
- Nullable versions of all above
- Arrays and `IEnumerable<T>` (comma-separated values)

### QueryNameAttribute

Overrides the query parameter name for a property.

```csharp
[QueryModel]
public class SearchQuery
{
    [QueryName("q")]
    public string SearchText { get; set; }

    [QueryName("page_size")]
    public int PageSize { get; set; }
}

// Maps from: ?q=hello&page_size=20
```

## Usage with ASP.NET Core

To use query model binding with ASP.NET Core, install the main package:

```bash
dotnet add package DevInstance.WebServiceToolkit
```

Then register the query model binder:

```csharp
builder.Services.AddControllers()
    .AddWebServiceToolkitQuery();
```

## See Also

- [DevInstance.WebServiceToolkit](https://www.nuget.org/packages/DevInstance.WebServiceToolkit) - Main package with ASP.NET Core integration
- [DevInstance.WebServiceToolkit.Database](https://www.nuget.org/packages/DevInstance.WebServiceToolkit.Database) - Database query interfaces
