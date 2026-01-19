# Pagination

WebServiceToolkit provides `ModelList<T>` for building paginated API responses.

## Overview

`ModelList<T>` is a wrapper class that includes:
- The items for the current page
- Pagination metadata (page, total count, page count)
- Sort information
- Search term

## ModelList<T> Properties

| Property | Type | Description |
|----------|------|-------------|
| `Items` | `T[]` | Array of items for the current page |
| `TotalCount` | `int` | Total items across all pages |
| `PagesCount` | `int` | Total number of pages |
| `Page` | `int` | Current page index (zero-based) |
| `Count` | `int` | Number of items in current page |
| `SortBy` | `string` | Column used for sorting |
| `IsAsc` | `bool` | True if ascending order |
| `Search` | `string` | Applied search query |

## Basic Usage

### Creating a Paginated Response

```csharp
public async Task<ModelList<Product>> GetProductsAsync(int page, int pageSize)
{
    var allProducts = await _repository.GetAllAsync();

    var totalCount = allProducts.Count;
    var items = allProducts
        .Skip(page * pageSize)
        .Take(pageSize)
        .ToArray();

    return new ModelList<Product>
    {
        Items = items,
        TotalCount = totalCount,
        PagesCount = (int)Math.Ceiling(totalCount / (double)pageSize),
        Page = page,
        Count = items.Length
    };
}
```

### With Sorting and Search

```csharp
public async Task<ModelList<Product>> GetProductsAsync(
    int page,
    int pageSize,
    string search,
    string sortBy,
    bool isAscending)
{
    var query = _repository.Query();

    // Apply search filter
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(p =>
            p.Name.Contains(search) ||
            p.Description.Contains(search));
    }

    // Get total count before pagination
    var totalCount = await query.CountAsync();

    // Apply sorting
    query = sortBy?.ToLower() switch
    {
        "name" => isAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
        "price" => isAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
        _ => query.OrderBy(p => p.Id)
    };

    // Apply pagination
    var items = await query
        .Skip(page * pageSize)
        .Take(pageSize)
        .ToArrayAsync();

    return new ModelList<Product>
    {
        Items = items,
        TotalCount = totalCount,
        PagesCount = (int)Math.Ceiling(totalCount / (double)pageSize),
        Page = page,
        Count = items.Length,
        SortBy = sortBy,
        IsAsc = isAscending,
        Search = search
    };
}
```

## Query Model Integration

Combine `ModelList<T>` with query models for clean controller endpoints:

### Query Model

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
}
```

### Controller

```csharp
[HttpGet]
public Task<ActionResult<ModelList<Product>>> GetProducts(ProductQuery query)
{
    return this.HandleWebRequestAsync<ModelList<Product>>(async () =>
    {
        var result = await _productService.GetProductsAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.SortBy,
            query.IsAscending);

        return Ok(result);
    });
}
```

### API Response Example

```json
GET /api/products?page=0&pageSize=10&search=widget&sort=name

{
  "items": [
    { "id": "abc123", "name": "Blue Widget", "price": 9.99 },
    { "id": "def456", "name": "Red Widget", "price": 14.99 }
  ],
  "totalCount": 25,
  "pagesCount": 3,
  "page": 0,
  "count": 10,
  "sortBy": "name",
  "isAsc": true,
  "search": "widget"
}
```

## Single Item Response

Use `ModelListResult.SingleItemList()` to wrap a single item:

```csharp
using DevInstance.WebServiceToolkit.Common.Tools;

[HttpGet("{id}")]
public Task<ActionResult<ModelList<Product>>> GetProduct(string id)
{
    return this.HandleWebRequestAsync<ModelList<Product>>(async () =>
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            throw new RecordNotFoundException(id);

        return Ok(ModelListResult.SingleItemList(product));
    });
}
```

Response:
```json
{
  "items": [{ "id": "abc123", "name": "Widget", "price": 9.99 }],
  "totalCount": 1,
  "pagesCount": 1,
  "page": 1,
  "count": 1
}
```

## Pagination Strategies

### Zero-Based Pages

WebServiceToolkit uses zero-based page indexing by default:

```csharp
// Page 0 = first page
var items = query.Skip(page * pageSize).Take(pageSize);
```

### One-Based Pages

If your API uses one-based pages, adjust the calculation:

```csharp
// Page 1 = first page
var items = query.Skip((page - 1) * pageSize).Take(pageSize);

return new ModelList<Product>
{
    Page = page,  // Return the 1-based page number
    // ...
};
```

### Cursor-Based Pagination

For cursor-based pagination, you might extend `ModelList<T>`:

```csharp
public class CursorModelList<T> : ModelList<T>
{
    public string? NextCursor { get; set; }
    public string? PreviousCursor { get; set; }
}
```

## Best Practices

### 1. Set Maximum Page Size

Prevent performance issues by enforcing a maximum page size:

```csharp
[QueryModel]
public class ProductQuery
{
    private int _pageSize = 20;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Min(value, 100);  // Max 100
    }
}
```

### 2. Count Before Pagination

Always get the total count before applying Skip/Take:

```csharp
// Good: Count on full query
var totalCount = await query.CountAsync();
var items = await query.Skip(skip).Take(take).ToListAsync();

// Bad: Count after pagination (always returns <= pageSize)
var items = await query.Skip(skip).Take(take).ToListAsync();
var totalCount = items.Count;
```

### 3. Include Pagination Info in Response

Always return full pagination metadata so clients can build UI:

```csharp
return new ModelList<Product>
{
    Items = items,
    TotalCount = totalCount,    // For "Showing 1-20 of 150"
    PagesCount = pagesCount,    // For page navigation
    Page = page,                // Current page indicator
    Count = items.Length        // Actual items returned
};
```

### 4. Handle Empty Results

```csharp
public async Task<ModelList<Product>> GetProductsAsync(ProductQuery query)
{
    var items = await ExecuteQueryAsync(query);

    return new ModelList<Product>
    {
        Items = items.ToArray(),
        TotalCount = items.Any() ? await CountAsync(query) : 0,
        PagesCount = items.Any() ? CalculatePages(query) : 0,
        Page = query.Page,
        Count = items.Count
    };
}
```

### 5. Use Clone() for Parallel Operations

When getting count and items, use separate query instances:

```csharp
var baseQuery = BuildQuery(filters);

// These can run in parallel without affecting each other
var countTask = baseQuery.Clone().CountAsync();
var itemsTask = baseQuery.Clone().Skip(skip).Take(take).ToListAsync();

await Task.WhenAll(countTask, itemsTask);

return new ModelList<Product>
{
    Items = itemsTask.Result.ToArray(),
    TotalCount = countTask.Result,
    // ...
};
```

## See Also

- [Getting Started](getting-started.md)
- [Query Model Binding](query-model-binding.md)
- [Database Queries](database-queries.md)
