using System;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>
/// Marks a POCO class as a query model for automatic query string parameter binding.
/// </summary>
/// <remarks>
/// <para>
/// When applied to a class, the <see cref="QueryModelAttribute"/> enables automatic binding
/// of HTTP query string parameters to the class's properties. This is used in conjunction
/// with the WebServiceToolkit query model binding infrastructure.
/// </para>
/// <para>
/// Properties in the marked class will be bound from query parameters with matching names.
/// Use <see cref="QueryNameAttribute"/> to override the parameter name for specific properties.
/// </para>
/// <para>
/// Supported property types include:
/// <list type="bullet">
/// <item><description>Primitive types: <c>string</c>, <c>bool</c>, <c>int</c>, <c>long</c>, <c>decimal</c>, <c>double</c></description></item>
/// <item><description>Date/time types: <c>DateTime</c>, <c>DateOnly</c>, <c>TimeOnly</c></description></item>
/// <item><description>Other types: <c>Guid</c>, enums</description></item>
/// <item><description>Nullable versions of the above types</description></item>
/// <item><description>Arrays and collections (comma-separated values)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [QueryModel]
/// public class ProductQuery
/// {
///     public int Page { get; set; }
///     public int PageSize { get; set; }
///     public string? Search { get; set; }
///
///     [QueryName("sort")]
///     public string? SortBy { get; set; }
/// }
///
/// // Usage in controller:
/// [HttpGet]
/// public ActionResult&lt;IEnumerable&lt;Product&gt;&gt; Get(ProductQuery query)
/// {
///     // query is automatically populated from ?page=1&amp;pageSize=20&amp;search=widget&amp;sort=name
/// }
/// </code>
/// </example>
/// <seealso cref="QueryNameAttribute"/>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class QueryModelAttribute : Attribute
{
}
