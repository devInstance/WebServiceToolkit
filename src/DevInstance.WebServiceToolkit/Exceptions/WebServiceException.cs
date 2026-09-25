namespace DevInstance.WebServiceToolkit.Exceptions;

/// <summary>
/// Base class for exceptions that map to a specific HTTP status code.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Controllers.ControllerUtils.HandleWebRequestAsync{T}"/> converts any exception deriving
/// from this class into a response with <see cref="StatusCode"/> and a
/// <see cref="Controllers.WebServiceError"/> body. Derive from it (or from one of the concrete
/// exceptions) to give domain exceptions their own status without changing controller code.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class PaymentRequiredException : WebServiceException
/// {
///     public PaymentRequiredException(string message) : base(402, message) { }
/// }
/// </code>
/// </example>
public abstract class WebServiceException : Exception
{
    /// <summary>
    /// Initializes a new instance with the HTTP status code to respond with.
    /// </summary>
    /// <param name="statusCode">The HTTP status code this exception maps to.</param>
    /// <param name="message">The error message sent to the client.</param>
    /// <param name="propertyName">Optional name of the request property that caused the error.</param>
    protected WebServiceException(int statusCode, string? message = null, string? propertyName = null)
        : base(message)
    {
        StatusCode = statusCode;
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the HTTP status code this exception maps to.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the name of the request property that caused the error, if any.
    /// </summary>
    public string? PropertyName { get; }
}
