# DevInstance.WebServiceToolkit.Common

Common models and attributes for WebServiceToolkit with no ASP.NET Core dependency.

## Installation

```bash
dotnet add package DevInstance.WebServiceToolkit.Common
```

## Features

This package provides:

- **Model interfaces** for API responses (`IModelItem`, `IModelList<T>`)
- **Legacy model classes** (`ModelItem`, `ModelList<T>`) — obsolete, kept for backward compatibility
- **Query model attributes** for parameter binding (`[QueryModel]`, `[QueryName]`)
- **Utility methods** for creating model responses

## API Reference

### IModelItem

Contract for entities with a server-assigned unique identifier.

```csharp
public class Product : IModelItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### IModelList&lt;T&gt;

Contract for paginated responses with sorting and search metadata.

```csharp
public class ProductList : IModelList<Product>
{
    public int TotalCount { get; set; }
    public int PagesCount { get; set; }
    public int Page { get; set; }
    public int Count { get; set; }
    public string[] SortOrder { get; set; }
    public string Search { get; set; }
    public Product[] Items { get; set; }
}
```

### ModelItem (obsolete)

> **Obsolete:** implement `IModelItem` instead. `ModelItem` implements `IModelItem` and is kept for backward compatibility.

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

### ModelList&lt;T&gt; (obsolete)

> **Obsolete:** implement `IModelList<T>` instead. `ModelList<T>` implements `IModelList<T>` and is kept for backward compatibility.

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

**Static Methods:**

| Method | Description |
|--------|-------------|
| `ModelList<T>.Empty()` | Creates an empty list with all counts set to zero |
| `ModelList<T>.Single(T item)` | Creates a list containing a single item |
| `ModelList<T>.FromArray(T[] items)` | Creates a list from an array with all items in one page |
| `ModelList<T>.IsEmpty(ModelList<T> list)` | Returns `true` if the list is null or has no items |

```csharp
// Empty list
var empty = ModelList<Product>.Empty();

// Single item
var single = ModelList<Product>.Single(product);

// From array (all items in one page)
var list = ModelList<Product>.FromArray(products);

// Check if empty or null
if (ModelList<Product>.IsEmpty(list))
{
    // handle empty case
}
```

### ModelListResult

Utility class for creating `IModelList<T>` responses. Pass your own `IModelList<T>` implementation as `TList`.

| Method | Description |
|--------|-------------|
| `CreateList<TList, T>(items, totalCount, top, page, sortOrder, search, useSearchMarkup)` | Builds a paginated response; calculates `PagesCount` from `top` and clamps `page` to the last page. With `useSearchMarkup`, wraps search matches in string properties in `<mark>` tags |
| `SingleItemList<TList, T>(item)` | Wraps a single item with all counts set to 1 |

```csharp
// Paginated response
return ModelListResult.CreateList<ProductList, Product>(
    items, totalCount: 150, top: 20, page: 0, sortOrder: new[] { "+Name" }, search: "widget");

// Single-item response
var product = await _repository.GetByIdAsync(id);
return ModelListResult.SingleItemList<ProductList, Product>(product);
```

The overloads without `TList` (`CreateList<T>`, `SingleItemList<T>`) return the obsolete `ModelList<T>` and are obsolete as well.

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
