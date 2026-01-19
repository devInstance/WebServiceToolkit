using System;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>
/// Overrides the query string parameter name used for binding a property.
/// </summary>
/// <remarks>
/// <para>
/// By default, query model binding maps query parameters to properties by matching
/// the parameter name to the property name (case-insensitive). Use this attribute
/// to specify a different query parameter name.
/// </para>
/// <para>
/// This is useful when:
/// <list type="bullet">
/// <item><description>The API uses different naming conventions than C# (e.g., snake_case)</description></item>
/// <item><description>You want shorter parameter names in URLs</description></item>
/// <item><description>The property name conflicts with reserved keywords</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [QueryModel]
/// public class SearchQuery
/// {
///     [QueryName("q")]
///     public string SearchText { get; set; }
///
///     [QueryName("page_size")]
///     public int PageSize { get; set; }
/// }
///
/// // Maps from: ?q=hello&amp;page_size=20
/// </code>
/// </example>
/// <seealso cref="QueryModelAttribute"/>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class QueryNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The query parameter name to use for binding.</param>
    public QueryNameAttribute(string name) => Name = name;

    /// <summary>
    /// Gets the query parameter name to use for binding.
    /// </summary>
    /// <value>The name of the query parameter in the URL.</value>
    public string Name { get; }
}
