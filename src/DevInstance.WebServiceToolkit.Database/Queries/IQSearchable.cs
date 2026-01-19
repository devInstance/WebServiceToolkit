namespace DevInstance.WebServiceToolkit.Database.Queries;

/// <summary>
/// Interface for query objects that support searching and lookup by identifier.
/// </summary>
/// <typeparam name="T">The derived query type for fluent API support.</typeparam>
/// <typeparam name="K">The type of the identifier (e.g., string, Guid, int).</typeparam>
/// <remarks>
/// <para>
/// Implement this interface to add search and lookup capabilities to your query objects.
/// The <see cref="ByPublicId"/> method filters to a specific entity, while
/// <see cref="Search"/> performs a text search across relevant fields.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Look up by ID
/// var product = await query.ByPublicId("abc123").FirstOrDefaultAsync();
///
/// // Search by keyword
/// var results = await query.Search("widget").Take(20).ExecuteAsync();
/// </code>
/// </example>
public interface IQSearchable<T, K>
{
    /// <summary>
    /// Filters the query to return only the entity with the specified public identifier.
    /// </summary>
    /// <param name="id">The public identifier to search for.</param>
    /// <returns>The query object for method chaining.</returns>
    /// <remarks>
    /// This method is typically used to retrieve a single entity by its public-facing ID,
    /// which may differ from the internal database ID.
    /// </remarks>
    T ByPublicId(K id);

    /// <summary>
    /// Applies a text search filter to the query.
    /// </summary>
    /// <param name="search">The search keyword or phrase.</param>
    /// <returns>The query object for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// The implementation determines which fields are searched and how matching is performed
    /// (e.g., contains, starts with, full-text search).
    /// </para>
    /// <para>
    /// Pass null or empty string to skip the search filter.
    /// </para>
    /// </remarks>
    T Search(string search);
}

/// <summary>
/// Interface for query objects that support searching and lookup by string identifier.
/// </summary>
/// <typeparam name="T">The derived query type for fluent API support.</typeparam>
/// <remarks>
/// This is a convenience interface equivalent to <see cref="IQSearchable{T,K}"/> with K as string.
/// </remarks>
public interface IQSearchable<T> : IQSearchable<T, string>
{
}