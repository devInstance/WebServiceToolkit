namespace DevInstance.WebServiceToolkit.Exceptions;

/// <summary>
/// Exception that represents an HTTP 400 Bad Request response.
/// </summary>
/// <remarks>
/// <para>
/// Throw this exception from your service layer when the client request is invalid or malformed.
/// When caught by <see cref="Controllers.ControllerUtils.HandleWebRequestAsync{T}"/>, it is automatically
/// converted to an HTTP 400 response with the exception message.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public async Task&lt;Product&gt; UpdateProductAsync(string id, ProductUpdateRequest request)
/// {
///     if (string.IsNullOrEmpty(request.Name))
///         throw new BadRequestException("Product name is required");
///
///     // ... update logic
/// }
/// </code>
/// </example>
/// <seealso cref="Controllers.ControllerUtils"/>
public class BadRequestException : WebServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class.
    /// </summary>
    public BadRequestException() : base(400, "Bad request")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public BadRequestException(string message) : base(400, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class for a specific request property.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="propertyName">The name of the request property that is invalid.</param>
    public BadRequestException(string message, string propertyName) : base(400, message, propertyName)
    {
    }
}
