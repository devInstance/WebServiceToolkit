using DevInstance.WebServiceToolkit.Common.Model;

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
}
