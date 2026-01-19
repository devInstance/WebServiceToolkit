using System;

namespace DevInstance.WebServiceToolkit.Common.Model;

/// <summary>
/// Represents a paginated, sortable, and searchable collection response.
/// </summary>
/// <typeparam name="T">The type of items contained in the list.</typeparam>
/// <remarks>
/// <para>
/// This class is used as a standard response DTO for API endpoints that return
/// collections with pagination support. It includes metadata about the current page,
/// total count, sorting, and search criteria.
/// </para>
/// <para>
/// Use <see cref="ModelListResult.SingleItemList{T}"/> to create a <see cref="ModelList{T}"/>
/// containing a single item when needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating a paginated response
/// var response = new ModelList&lt;Product&gt;
/// {
///     Items = products.ToArray(),
///     TotalCount = totalProducts,
///     PagesCount = (int)Math.Ceiling(totalProducts / (double)pageSize),
///     Page = currentPage,
///     Count = products.Length,
///     SortBy = "Name",
///     IsAsc = true
/// };
/// </code>
/// </example>
public class ModelList<T>
{
    /// <summary>
    /// Gets or sets the total count of items across all pages.
    /// </summary>
    /// <value>The total number of items matching the query criteria.</value>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages available.
    /// </summary>
    /// <value>The total page count based on the page size.</value>
    public int PagesCount { get; set; }

    /// <summary>
    /// Gets or sets the current page index (zero-based).
    /// </summary>
    /// <value>The zero-based index of the current page.</value>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items in the current page.
    /// </summary>
    /// <value>The count of items returned in this response.</value>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the name of the column used for sorting.
    /// </summary>
    /// <value>The property name by which the results are sorted, or null if not sorted.</value>
    public string SortBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sort order is ascending.
    /// </summary>
    /// <value><c>true</c> if sorted in ascending order; <c>false</c> for descending order.</value>
    public bool IsAsc { get; set; }

    /// <summary>
    /// Gets or sets the search string used to filter results.
    /// </summary>
    /// <value>The search query applied to the results, or null if no search was applied.</value>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the filter value.
    /// </summary>
    /// <value>The filter bitmask value.</value>
    [Obsolete("This property is deprecated and will be removed in a future version.")]
    public int Filter { get; set; }

    /// <summary>
    /// Gets or sets the fields to include in the response.
    /// </summary>
    /// <value>The fields bitmask value.</value>
    [Obsolete("This property is deprecated and will be removed in a future version.")]
    public int Fields { get; set; }

    /// <summary>
    /// Gets or sets the array of items for the current page.
    /// </summary>
    /// <value>An array of items of type <typeparamref name="T"/>.</value>
    public T[] Items { get; set; }
}