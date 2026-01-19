namespace DevInstance.WebServiceToolkit.Exceptions;

/// <summary>
/// Exception that represents an HTTP 404 Not Found response.
/// </summary>
/// <remarks>
/// <para>
/// Throw this exception from your service layer when a requested resource cannot be found.
/// When caught by <see cref="Controllers.ControllerUtils.HandleWebRequestAsync{T}"/>, it is automatically
/// converted to an HTTP 404 response.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public async Task&lt;Product&gt; GetProductAsync(string id)
/// {
///     var product = await _repository.FindByIdAsync(id);
///     if (product == null)
///         throw new RecordNotFoundException(id);
///
///     return product;
/// }
/// </code>
/// </example>
/// <seealso cref="Controllers.ControllerUtils"/>
public class RecordNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordNotFoundException"/> class.
    /// </summary>
    public RecordNotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordNotFoundException"/> class with a specified identifier.
    /// </summary>
    /// <param name="message">The identifier or description of the resource that was not found.</param>
    public RecordNotFoundException(string message) : base($"Record not found: {message}")
    {
    }
}
