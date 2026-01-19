# DevInstance.WebServiceToolkit.Database

Interfaces for building fluent database query objects with pagination, search, and sorting support.

## Installation

```bash
dotnet add package DevInstance.WebServiceToolkit.Database
```

## Features

This package provides interfaces for implementing the Query Object pattern:

- **IModelQuery<T,D>** - Base interface with CRUD operations
- **IQPageable<T>** - Skip/Take pagination support
- **IQSearchable<T,K>** - Search and ID lookup support
- **IQSortable<T>** - Column-based sorting support

## API Reference

### IModelQuery<T, D>

Base interface for query objects that encapsulate database operations.

```csharp
public interface IModelQuery<T, D>
{
    T CreateNew();
    Task AddAsync(T record);
    Task UpdateAsync(T record);
    Task RemoveAsync(T record);
    D Clone();
}
```

**Type Parameters:**
- `T` - The entity type being queried
- `D` - The derived query type (for fluent API support)

### IQPageable<T>

Interface for pagination support.

```csharp
public interface IQPageable<T>
{
    T Skip(int value);
    T Take(int value);
}
```

**Usage:**
```csharp
// Get page 2 with 20 items per page
var results = await query.Skip(20).Take(20).ExecuteAsync();
```

### IQSearchable<T, K>

Interface for search and lookup operations.

```csharp
public interface IQSearchable<T, K>
{
    T ByPublicId(K id);
    T Search(string search);
}

// Convenience interface for string IDs
public interface IQSearchable<T> : IQSearchable<T, string> { }
```

**Usage:**
```csharp
// Look up by ID
var product = await query.ByPublicId("abc123").FirstOrDefaultAsync();

// Search by keyword
var results = await query.Search("widget").Take(20).ExecuteAsync();
```

### IQSortable<T>

Interface for sorting support.

```csharp
public interface IQSortable<T>
{
    T SortBy(string column, bool isAsc);
    string SortedBy { get; }
    bool IsAsc { get; }
}
```

**Usage:**
```csharp
// Sort by name ascending
var results = await query.SortBy("Name", isAsc: true).ExecuteAsync();

// Check current sort state
Console.WriteLine($"Sorted by: {query.SortedBy}, Ascending: {query.IsAsc}");
```

## Implementation Example

```csharp
using DevInstance.WebServiceToolkit.Database.Queries;

public class ProductQuery :
    IModelQuery<Product, ProductQuery>,
    IQPageable<ProductQuery>,
    IQSearchable<ProductQuery>,
    IQSortable<ProductQuery>
{
    private readonly AppDbContext _context;
    private IQueryable<Product> _query;
    private int _skip;
    private int _take = int.MaxValue;

    public string SortedBy { get; private set; }
    public bool IsAsc { get; private set; } = true;

    public ProductQuery(AppDbContext context)
    {
        _context = context;
        _query = context.Products;
    }

    // IModelQuery implementation
    public Product CreateNew() => new Product();

    public async Task AddAsync(Product record)
    {
        await _context.Products.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product record)
    {
        _context.Products.Update(record);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Product record)
    {
        _context.Products.Remove(record);
        await _context.SaveChangesAsync();
    }

    public ProductQuery Clone() => new ProductQuery(_context)
    {
        _query = _query,
        _skip = _skip,
        _take = _take,
        SortedBy = SortedBy,
        IsAsc = IsAsc
    };

    // IQPageable implementation
    public ProductQuery Skip(int value)
    {
        _skip = value;
        return this;
    }

    public ProductQuery Take(int value)
    {
        _take = value;
        return this;
    }

    // IQSearchable implementation
    public ProductQuery ByPublicId(string id)
    {
        _query = _query.Where(p => p.PublicId == id);
        return this;
    }

    public ProductQuery Search(string search)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            _query = _query.Where(p =>
                p.Name.Contains(search) ||
                p.Description.Contains(search));
        }
        return this;
    }

    // IQSortable implementation
    public ProductQuery SortBy(string column, bool isAsc)
    {
        SortedBy = column;
        IsAsc = isAsc;

        _query = column?.ToLower() switch
        {
            "name" => isAsc ? _query.OrderBy(p => p.Name) : _query.OrderByDescending(p => p.Name),
            "price" => isAsc ? _query.OrderBy(p => p.Price) : _query.OrderByDescending(p => p.Price),
            _ => _query.OrderBy(p => p.Id)
        };
        return this;
    }

    // Execute the query
    public async Task<List<Product>> ExecuteAsync()
    {
        return await _query.Skip(_skip).Take(_take).ToListAsync();
    }

    public async Task<Product?> FirstOrDefaultAsync()
    {
        return await _query.FirstOrDefaultAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _query.CountAsync();
    }
}
```

## Integration with ModelList

Use these query interfaces with `ModelList<T>` from the Common package:

```csharp
public async Task<ModelList<Product>> GetProductsAsync(int page, int pageSize, string search, string sortBy, bool isAsc)
{
    var query = _productQuery
        .Search(search)
        .SortBy(sortBy, isAsc);

    var totalCount = await query.Clone().CountAsync();

    var items = await query
        .Skip(page * pageSize)
        .Take(pageSize)
        .ExecuteAsync();

    return new ModelList<Product>
    {
        Items = items.ToArray(),
        TotalCount = totalCount,
        PagesCount = (int)Math.Ceiling(totalCount / (double)pageSize),
        Page = page,
        Count = items.Count,
        SortBy = sortBy,
        IsAsc = isAsc,
        Search = search
    };
}
```

## See Also

- [DevInstance.WebServiceToolkit](https://www.nuget.org/packages/DevInstance.WebServiceToolkit) - Main package with ASP.NET Core integration
- [DevInstance.WebServiceToolkit.Common](https://www.nuget.org/packages/DevInstance.WebServiceToolkit.Common) - Common models including ModelList<T>
