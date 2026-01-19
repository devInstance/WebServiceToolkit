# Database Queries

WebServiceToolkit.Database provides interfaces for building fluent query objects that encapsulate database access patterns.

## Overview

The Query Object pattern separates query construction from execution, enabling:

- Fluent method chaining
- Reusable query components
- Testable data access
- Clean separation of concerns

## Interfaces

### IModelQuery<T, D>

Base interface for query objects with CRUD operations:

```csharp
public interface IModelQuery<T, D>
{
    T CreateNew();           // Factory method for new entities
    Task AddAsync(T record); // Add entity to data store
    Task UpdateAsync(T record); // Update existing entity
    Task RemoveAsync(T record); // Remove entity
    D Clone();               // Copy query state
}
```

### IQPageable<T>

Adds pagination support:

```csharp
public interface IQPageable<T>
{
    T Skip(int value);  // Skip N items
    T Take(int value);  // Take N items
}
```

### IQSearchable<T, K>

Adds search and lookup capabilities:

```csharp
public interface IQSearchable<T, K>
{
    T ByPublicId(K id);    // Filter by ID
    T Search(string search); // Text search
}

// Convenience interface for string IDs
public interface IQSearchable<T> : IQSearchable<T, string> { }
```

### IQSortable<T>

Adds sorting support:

```csharp
public interface IQSortable<T>
{
    T SortBy(string column, bool isAsc);
    string SortedBy { get; }
    bool IsAsc { get; }
}
```

## Implementation Example

Here's a complete implementation using Entity Framework Core:

```csharp
using DevInstance.WebServiceToolkit.Database.Queries;
using Microsoft.EntityFrameworkCore;

public class ProductQuery :
    IModelQuery<Product, ProductQuery>,
    IQPageable<ProductQuery>,
    IQSearchable<ProductQuery>,
    IQSortable<ProductQuery>
{
    private readonly AppDbContext _context;
    private IQueryable<Product> _query;
    private int _skip = 0;
    private int _take = int.MaxValue;

    public string SortedBy { get; private set; }
    public bool IsAsc { get; private set; } = true;

    public ProductQuery(AppDbContext context)
    {
        _context = context;
        _query = context.Products.AsQueryable();
    }

    // Private constructor for cloning
    private ProductQuery(AppDbContext context, IQueryable<Product> query,
        int skip, int take, string sortedBy, bool isAsc)
    {
        _context = context;
        _query = query;
        _skip = skip;
        _take = take;
        SortedBy = sortedBy;
        IsAsc = isAsc;
    }

    // IModelQuery<T, D> implementation
    public Product CreateNew() => new Product { Id = Guid.NewGuid().ToString() };

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

    public ProductQuery Clone() => new ProductQuery(
        _context, _query, _skip, _take, SortedBy, IsAsc);

    // IQPageable<T> implementation
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

    // IQSearchable<T> implementation
    public ProductQuery ByPublicId(string id)
    {
        _query = _query.Where(p => p.Id == id);
        return this;
    }

    public ProductQuery Search(string search)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            _query = _query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term) ||
                p.Category.ToLower().Contains(term));
        }
        return this;
    }

    // IQSortable<T> implementation
    public ProductQuery SortBy(string column, bool isAsc)
    {
        SortedBy = column;
        IsAsc = isAsc;

        _query = (column?.ToLower()) switch
        {
            "name" => isAsc
                ? _query.OrderBy(p => p.Name)
                : _query.OrderByDescending(p => p.Name),
            "price" => isAsc
                ? _query.OrderBy(p => p.Price)
                : _query.OrderByDescending(p => p.Price),
            "category" => isAsc
                ? _query.OrderBy(p => p.Category)
                : _query.OrderByDescending(p => p.Category),
            "created" => isAsc
                ? _query.OrderBy(p => p.CreatedAt)
                : _query.OrderByDescending(p => p.CreatedAt),
            _ => _query.OrderBy(p => p.Id)
        };

        return this;
    }

    // Additional query methods
    public ProductQuery ByCategory(string category)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            _query = _query.Where(p => p.Category == category);
        }
        return this;
    }

    public ProductQuery InPriceRange(decimal? min, decimal? max)
    {
        if (min.HasValue)
            _query = _query.Where(p => p.Price >= min.Value);
        if (max.HasValue)
            _query = _query.Where(p => p.Price <= max.Value);
        return this;
    }

    // Execution methods
    public async Task<List<Product>> ToListAsync()
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

## Using the Query Object

### Basic Usage

```csharp
public class ProductService : IProductService
{
    private readonly ProductQuery _query;

    public ProductService(AppDbContext context)
    {
        _query = new ProductQuery(context);
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _query.Clone()
            .ByPublicId(id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Product>> SearchAsync(string term)
    {
        return await _query.Clone()
            .Search(term)
            .SortBy("name", isAsc: true)
            .Take(20)
            .ToListAsync();
    }
}
```

### Paginated Results

```csharp
public async Task<ModelList<Product>> GetProductsAsync(ProductQueryParams queryParams)
{
    var query = _query.Clone()
        .Search(queryParams.Search)
        .ByCategory(queryParams.Category)
        .InPriceRange(queryParams.MinPrice, queryParams.MaxPrice)
        .SortBy(queryParams.SortBy ?? "name", queryParams.IsAscending);

    // Count total before pagination
    var totalCount = await query.Clone().CountAsync();

    // Apply pagination
    var items = await query
        .Skip(queryParams.Page * queryParams.PageSize)
        .Take(queryParams.PageSize)
        .ToListAsync();

    return new ModelList<Product>
    {
        Items = items.ToArray(),
        TotalCount = totalCount,
        PagesCount = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize),
        Page = queryParams.Page,
        Count = items.Count,
        SortBy = query.SortedBy,
        IsAsc = query.IsAsc,
        Search = queryParams.Search
    };
}
```

## Clone() Pattern

The `Clone()` method is essential for reusing query objects without affecting the original state:

```csharp
// Base query with common filters
var baseQuery = _query.Clone()
    .Search("widget")
    .ByCategory("electronics");

// Branch for first page
var page1 = baseQuery.Clone().Skip(0).Take(10).ToListAsync();

// Branch for second page (doesn't affect page1 query)
var page2 = baseQuery.Clone().Skip(10).Take(10).ToListAsync();

// Branch for count (doesn't include pagination)
var count = baseQuery.Clone().CountAsync();

// Execute all in parallel
await Task.WhenAll(page1, page2, count);
```

## Testing

Query objects make testing easier:

```csharp
public class ProductQueryTests
{
    [Fact]
    public async Task Search_FiltersProducts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        using var context = new AppDbContext(options);
        context.Products.AddRange(
            new Product { Id = "1", Name = "Widget", Category = "Tools" },
            new Product { Id = "2", Name = "Gadget", Category = "Electronics" }
        );
        await context.SaveChangesAsync();

        var query = new ProductQuery(context);

        // Act
        var results = await query.Search("widget").ToListAsync();

        // Assert
        Assert.Single(results);
        Assert.Equal("Widget", results[0].Name);
    }
}
```

## Best Practices

1. **Always Clone** before modifying a query:
   ```csharp
   // Good
   var result = await _query.Clone().ByPublicId(id).FirstOrDefaultAsync();

   // Risky - modifies the shared query
   var result = await _query.ByPublicId(id).FirstOrDefaultAsync();
   ```

2. **Keep queries immutable** - each method should return `this` or a new instance, never modify shared state.

3. **Add domain-specific methods** for common filters:
   ```csharp
   public ProductQuery ActiveOnly() =>
       _query = _query.Where(p => !p.IsDeleted);
   ```

4. **Compose queries** by combining interface implementations:
   ```csharp
   public interface IProductQuery :
       IModelQuery<Product, IProductQuery>,
       IQPageable<IProductQuery>,
       IQSearchable<IProductQuery>,
       IQSortable<IProductQuery>
   { }
   ```

## See Also

- [Getting Started](getting-started.md)
- [Pagination](pagination.md)
