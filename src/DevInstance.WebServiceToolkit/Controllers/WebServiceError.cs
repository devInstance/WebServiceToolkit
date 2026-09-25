namespace DevInstance.WebServiceToolkit.Controllers;

/// <summary>
/// Category of a <see cref="WebServiceError"/>.
/// </summary>
/// <remarks>
/// Values are numerically identical to DevInstance.BlazorToolkit's <c>ServiceActionErrorType</c>,
/// so a Blazor client deserializes the error body without a mapping step. Do not renumber.
/// </remarks>
public enum WebServiceErrorType
{
    /// <summary>Unclassified error.</summary>
    Unknown = 0,

    /// <summary>Unhandled server-side exception (HTTP 500).</summary>
    Exception = 1,

    /// <summary>A handled domain error (404, 409, 401, 403, ...).</summary>
    General = 2,

    /// <summary>The request was invalid (400, 422), optionally for a specific property.</summary>
    Validation = 3,
}

/// <summary>
/// JSON body returned for every error response produced by <see cref="ControllerUtils"/>.
/// </summary>
/// <remarks>
/// Wire-compatible with DevInstance.BlazorToolkit's <c>ServiceActionError</c>
/// (<c>errorType</c>, <c>message</c>, <c>propertyName</c>), which its <c>HttpApiContext</c>
/// reads from any non-success response.
/// </remarks>
public class WebServiceError
{
    /// <summary>Gets or sets the error category.</summary>
    public WebServiceErrorType ErrorType { get; set; } = WebServiceErrorType.Unknown;

    /// <summary>Gets or sets a message safe to show to the client.</summary>
    public string? Message { get; set; }

    /// <summary>Gets or sets the name of the request property that caused the error, if any.</summary>
    public string? PropertyName { get; set; }
}
