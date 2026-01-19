namespace DevInstance.WebServiceToolkit.Database.Queries;

/// <summary>
/// Interface for query objects that support pagination via Skip/Take operations.
/// </summary>
/// <typeparam name="T">The derived query type for fluent API support.</typeparam>
/// <remarks>
/// <para>
/// Implement this interface to add pagination support to your query objects.
/// The methods return the query object itself to support method chaining.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get second page of 20 items
/// var results = await query
///     .Search("widget")
///     .Skip(20)
///     .Take(20)
///     .ExecuteAsync();
/// </code>
/// </example>
public interface IQPageable<T>
{
    /// <summary>
    /// Skips the specified number of items in the result sequence.
    /// </summary>
    /// <param name="value">The number of items to skip. Use 0 to start from the beginning.</param>
    /// <returns>The query object for method chaining.</returns>
    /// <remarks>
    /// Use in combination with <see cref="Take"/> for pagination. For page N with page size P,
    /// use <c>Skip((N - 1) * P).Take(P)</c> for 1-based pages, or <c>Skip(N * P).Take(P)</c> for 0-based pages.
    /// </remarks>
    T Skip(int value);

    /// <summary>
    /// Takes the specified number of items from the result sequence.
    /// </summary>
    /// <param name="value">The maximum number of items to return.</param>
    /// <returns>The query object for method chaining.</returns>
    /// <remarks>
    /// This is typically used as the page size in pagination scenarios.
    /// </remarks>
    T Take(int value);
}
