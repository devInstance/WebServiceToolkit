namespace DevInstance.WebServiceToolkit.Database.Queries.Extensions;

public static class IQPageableExtensions
{
    /// <summary>
    /// Applies pagination to a query by setting the page size and page number.
    /// </summary>
    /// <typeparam name="T">The query type that implements <see cref="IQPageable{T}"/>.</typeparam>
    /// <param name="q">The query to apply pagination to.</param>
    /// <param name="size">The number of items per page. If null or less than 1, no pagination is applied.</param>
    /// <param name="page">The zero-based page number. If null or less than 1, starts from the first page.</param>
    /// <returns>The query with pagination applied.</returns>
    /// <remarks>
    /// <para>
    /// This method applies pagination in two steps:
    /// <list type="number">
    /// <item>If both <paramref name="size"/> and <paramref name="page"/> are valid, it skips <c>page * size</c> items.</item>
    /// <item>If <paramref name="size"/> is valid, it takes <paramref name="size"/> items.</item>
    /// </list>
    /// </para>
    /// <para>
    /// If <paramref name="size"/> is null or zero, the query is returned unchanged without pagination.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var query = myQuery.ApplyPages(size: 10, page: 2); // Returns items 20-29
    /// </code>
    /// </example>
    public static T Paginate<T>(this T q, int? size, int? page) where T : IQPageable<T>
    {
        if (size.HasValue && size.Value > 0)
        {
            if (page.HasValue && page.Value > 0)
            {
                q = q.Skip(page.Value * size.Value);
            }
            q = q.Take(size.Value);
        }

        return q;
    }

}
