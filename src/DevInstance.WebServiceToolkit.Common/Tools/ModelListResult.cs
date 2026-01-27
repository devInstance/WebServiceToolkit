using DevInstance.WebServiceToolkit.Common.Model;
using System;

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
    /// Creates a new instance of the ModelList<T> class containing the specified items and optional pagination
    /// metadata.
    /// </summary>
    /// <remarks>If the specified page index exceeds the total number of pages, the last page is used. The
    /// method does not perform validation on the contents of the items array.</remarks>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="items">The array of items to include in the list. Cannot be null.</param>
    /// <param name="totalCount">The total number of items available across all pages. If null, the length of items is used.</param>
    /// <param name="top">The maximum number of items per page. If null or less than or equal to zero, pagination is not applied.</param>
    /// <param name="page">The zero-based index of the current page. If null, defaults to the first page.</param>
    /// <returns>A ModelList<T> instance containing the specified items and pagination information.</returns>
    public static ModelList<T> CreateList<T>(T[] items, int? totalCount = null, int? top = null, int? page = null)
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

        return new ModelList<T>()
        {
            TotalCount = totalItemsCount,
            Count = items.Length,
            PagesCount = totalPageCount,
            Page = pageIndex,
            Items = items
        };
    }

}
