using System.Threading.Tasks;

namespace DevInstance.WebServiceToolkit.Database.Queries;

/// <summary>
/// Base interface for database query objects that support CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
/// <typeparam name="D">The derived query type (for fluent API support via Clone).</typeparam>
/// <remarks>
/// <para>
/// Implement this interface to create query objects that encapsulate database access patterns.
/// The interface supports a fluent API pattern where methods return the derived type <typeparamref name="D"/>
/// to allow method chaining.
/// </para>
/// <para>
/// Combine with <see cref="IQPageable{T}"/>, <see cref="IQSearchable{T,K}"/>, and <see cref="IQSortable{T}"/>
/// to build feature-rich query objects.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class ProductQuery : IModelQuery&lt;Product, ProductQuery&gt;, IQPageable&lt;ProductQuery&gt;
/// {
///     public Product CreateNew() => new Product();
///     public Task AddAsync(Product record) => _context.Products.AddAsync(record).AsTask();
///     public Task UpdateAsync(Product record) { _context.Products.Update(record); return Task.CompletedTask; }
///     public Task RemoveAsync(Product record) { _context.Products.Remove(record); return Task.CompletedTask; }
///     public ProductQuery Clone() => new ProductQuery(_context) { /* copy state */ };
/// }
/// </code>
/// </example>
public interface IModelQuery<T, D>
{
    /// <summary>
    /// Creates a new instance of the entity type.
    /// </summary>
    /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// Use this factory method to create new entities that will be added to the data store.
    /// This allows the query object to initialize entity properties or set default values.
    /// </remarks>
    T CreateNew();

    /// <summary>
    /// Asynchronously adds a new entity to the data store.
    /// </summary>
    /// <param name="record">The entity to add.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(T record);

    /// <summary>
    /// Asynchronously updates an existing entity in the data store.
    /// </summary>
    /// <param name="record">The entity to update.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateAsync(T record);

    /// <summary>
    /// Asynchronously removes an entity from the data store.
    /// </summary>
    /// <param name="record">The entity to remove.</param>
    /// <returns>A task representing the asynchronous remove operation.</returns>
    Task RemoveAsync(T record);

    /// <summary>
    /// Creates a deep copy of this query object with its current state.
    /// </summary>
    /// <returns>A new instance of type <typeparamref name="D"/> with the same state as this query.</returns>
    /// <remarks>
    /// <para>
    /// Use this method when you need to create a modified query without affecting the original.
    /// This is useful for branching queries with different filter conditions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var baseQuery = query.Search("widget");
    /// var page1 = baseQuery.Clone().Skip(0).Take(10);
    /// var page2 = baseQuery.Clone().Skip(10).Take(10);
    /// </code>
    /// </example>
    D Clone();
}
