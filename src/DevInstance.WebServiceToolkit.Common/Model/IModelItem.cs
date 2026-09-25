namespace DevInstance.WebServiceToolkit.Common.Model;

/// <summary>
/// Contract for model entities that have a server-assigned unique identifier.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface on DTOs (Data Transfer Objects) that represent
/// entities with a unique identifier managed by the server.
/// </para>
/// <para>
/// The <see cref="Id"/> property is typically set by the server when creating new records
/// and should be used for subsequent operations like updates and deletes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Product : IModelItem
/// {
///     public string Id { get; set; }
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// </code>
/// </example>
public interface IModelItem
{
    /// <summary>
    /// Gets or sets the unique public identifier assigned by the server.
    /// </summary>
    /// <value>
    /// A string representing the unique identifier. This value is typically
    /// assigned by the server when creating a new record.
    /// </value>
    string Id { get; set; }
}
