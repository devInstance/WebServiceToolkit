using DevInstance.WebServiceToolkit.Common.Model;
using System;
using System.Globalization;

namespace DevInstance.WebServiceToolkit.Common.Tools;

/// <summary>
/// Provides utility methods for creating <see cref="ModelList{T}"/> instances.
/// </summary>
public static class ModelListResult
{
    /// <summary>
    /// Creates a <see cref="ModelList{T}"/> containing a single item.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="item">The item to wrap in a list response.</param>
    /// <returns>
    /// A <see cref="ModelList{T}"/> with <see cref="ModelList{T}.TotalCount"/>,
    /// <see cref="ModelList{T}.PagesCount"/>, and <see cref="ModelList{T}.Count"/>
    /// all set to 1.
    /// </returns>
    /// <example>
    /// <code>
    /// var product = await _productService.GetByIdAsync(id);
    /// return ModelListResult.SingleItemList(product);
    /// </code>
    /// </example>
    public static ModelList<T> SingleItemList<T>(T item)
    {
        return new ModelList<T>
        {
            Count = 1,
            Items = new T[] { item },
            Page = 1,
            PagesCount = 1,
            TotalCount = 1,
        };
    }


    /// <summary>
    /// Creates a paginated list of items with optional sorting and search capabilities, returning a ModelList<T> that
    /// includes pagination metadata and the resulting items.
    /// </summary>
    /// <remarks>If useSearchMarkup is set to true and a search term is provided, occurrences of the search
    /// term within string properties of the items are wrapped in <mark> tags for highlighting. The method does not
    /// perform actual filtering; it only highlights matches. The returned ModelList<T> includes metadata such as total
    /// item count, page count, current page, sort order, and search term for client-side use.</remarks>
    /// <typeparam name="T">The type of the items contained in the list.</typeparam>
    /// <param name="items">An array of items to include in the resulting list. Cannot be null.</param>
    /// <param name="totalCount">The total number of items available across all pages. If not specified, the length of the items array is used.</param>
    /// <param name="top">The maximum number of items to include per page. If not specified or less than or equal to zero, all items are
    /// included in a single page.</param>
    /// <param name="page">The zero-based index of the page to retrieve. If the specified page exceeds the total number of pages, the last
    /// page is returned.</param>
    /// <param name="sortOrder">An array of strings specifying the sort order for the items. Each string typically represents a property name
    /// and sort direction. The sort direction is represented by a prefix "+" for ascending or "-" for descending.</param>
    /// <param name="search">A search term used to filter or highlight items. If specified, items containing the search term may be
    /// highlighted depending on the value of useSearchMarkup.</param>
    /// <param name="useSearchMarkup">true to highlight occurrences using <mark>...</mark> tags of the search term within item properties using markup; otherwise, false.</param>
    /// <returns>A ModelList<T> containing the paginated, optionally sorted and searched items, along with pagination and search
    /// metadata.</returns>
    public static ModelList<T> CreateList<T>(T[] items, int? totalCount = null, int? top = null, int? page = null, string[] sortOrder = null, string search = null, bool useSearchMarkup = false)
    {
        var totalItemsCount = totalCount ?? items.Length;

        var pageIndex = 0;
        var totalPageCount = 1;
        if (top.HasValue && top.Value > 0)
        {
            totalPageCount = (int)Math.Ceiling((double)totalItemsCount / (double)top.Value);
        }
        if (page.HasValue && page.Value >= 0)
        {
            pageIndex = page.Value;
            if (pageIndex >= totalPageCount)
            {
                pageIndex = totalPageCount - 1;
            }
        }

        if(useSearchMarkup && !string.IsNullOrEmpty(search))
        {
            var searchTerm = search.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (item == null) continue;

                    var type = typeof(T);
                    var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    foreach (var prop in properties)
                    {
                        if (!prop.CanRead || !prop.CanWrite) continue;

                        var value = prop.GetValue(item);
                        if (value == null) continue;

                        var valueString = value.ToString();
                        if (string.IsNullOrEmpty(valueString)) continue;

                        if (valueString.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            // Wrap the matching term in <mark> tags for highlighting
                            var highlightedString = System.Text.RegularExpressions.Regex.Replace(
                                valueString,
                                System.Text.RegularExpressions.Regex.Escape(searchTerm),
                                match => $"<mark>{match.Value}</mark>",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            try
                            {
                                var propertyType = prop.PropertyType;
                                if (propertyType == typeof(string))
                                {
                                    prop.SetValue(item, highlightedString);
                                }
                                else
                                {
                                    var convertedValue = Convert.ChangeType(highlightedString, propertyType, CultureInfo.InvariantCulture);
                                    prop.SetValue(item, convertedValue);
                                }
                            }
                            catch
                            {
                                // If conversion fails, keep the original value
                            }
                        }
                    }
                }
            }
        }

        return new ModelList<T>()
        {
            TotalCount = totalItemsCount,
            Count = items.Length,
            PagesCount = totalPageCount,
            Page = pageIndex,
            SortOrder = sortOrder,
            Search = search,
            Items = items
        };
    }

}
