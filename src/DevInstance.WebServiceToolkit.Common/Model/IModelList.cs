namespace DevInstance.WebServiceToolkit.Common.Model;

/// <summary>
/// Contract for a paginated, sortable, and searchable collection response.
/// </summary>
/// <typeparam name="T">The type of items contained in the list.</typeparam>
/// <remarks>
/// Implement this interface on response DTOs for API endpoints that return
/// collections with pagination support. It describes the current page,
/// total count, sorting, and search criteria.
/// </remarks>
/// <example>
/// <code>
/// public class ProductList : IModelList&lt;Product&gt;
/// {
///     public int TotalCount { get; set; }
///     public int PagesCount { get; set; }
///     public int Page { get; set; }
///     public int Count { get; set; }
///     public string[] SortOrder { get; set; }
///     public string Search { get; set; }
///     public Product[] Items { get; set; }
/// }
/// </code>
/// </example>
public interface IModelList<T>
{
    /// <summary>
    /// Gets or sets the total count of items across all pages.
    /// </summary>
    /// <value>The total number of items matching the query criteria.</value>
    int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages available.
    /// </summary>
    /// <value>The total page count based on the page size.</value>
    int PagesCount { get; set; }

    /// <summary>
    /// Gets or sets the current page index (zero-based).
    /// </summary>
    /// <value>The zero-based index of the current page.</value>
    int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items in the current page.
    /// </summary>
    /// <value>The count of items returned in this response.</value>
    int Count { get; set; }

    /// <summary>
    /// Gets or sets the order in which items are sorted.
    /// </summary>
    /// <remarks>
    /// Each string represents a sorting criterion: a "+" prefix indicates ascending order,
    /// a "-" prefix indicates descending order, and the position in the array determines
    /// the sorting priority (first element has highest priority).
    /// </remarks>
    string[] SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the search string used to filter results.
    /// </summary>
    /// <value>The search query applied to the results, or null if no search was applied.</value>
    string Search { get; set; }

    /// <summary>
    /// Gets or sets the array of items for the current page.
    /// </summary>
    /// <value>An array of items of type <typeparamref name="T"/>.</value>
    T[] Items { get; set; }
}
