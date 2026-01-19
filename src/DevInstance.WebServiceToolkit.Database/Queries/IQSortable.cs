namespace DevInstance.WebServiceToolkit.Database.Queries;

/// <summary>
/// Interface for query objects that support sorting by column name.
/// </summary>
/// <typeparam name="T">The derived query type for fluent API support.</typeparam>
/// <remarks>
/// <para>
/// Implement this interface to add sorting capabilities to your query objects.
/// The <see cref="SortBy"/> method configures the sort, while <see cref="SortedBy"/>
/// and <see cref="IsAsc"/> properties report the current sort state.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Sort by name ascending
/// var results = await query.SortBy("Name", isAsc: true).ExecuteAsync();
///
/// // Sort by price descending
/// var results = await query.SortBy("Price", isAsc: false).ExecuteAsync();
/// </code>
/// </example>
public interface IQSortable<T>
{
    /// <summary>
    /// Applies sorting to the query by the specified column.
    /// </summary>
    /// <param name="column">The name of the column/property to sort by.</param>
    /// <param name="isAsc"><c>true</c> for ascending order; <c>false</c> for descending order.</param>
    /// <returns>The query object for method chaining.</returns>
    /// <remarks>
    /// The column name should match a property name on the entity type.
    /// Implementations may support case-insensitive matching.
    /// </remarks>
    T SortBy(string column, bool isAsc);

    /// <summary>
    /// Gets the name of the column currently used for sorting.
    /// </summary>
    /// <value>The column name, or null if no sorting is applied.</value>
    string SortedBy { get; }

    /// <summary>
    /// Gets a value indicating whether the current sort order is ascending.
    /// </summary>
    /// <value><c>true</c> if sorting in ascending order; <c>false</c> for descending order.</value>
    bool IsAsc { get; }
}
