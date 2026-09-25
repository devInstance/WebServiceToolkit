namespace DevInstance.WebServiceToolkit.Exceptions;

/// <summary>
/// Exception that represents an HTTP 403 Forbidden response.
/// </summary>
/// <remarks>
/// Throw this when the caller is authenticated but not allowed to perform the operation on the
/// requested resource (e.g. editing another user's record). Use <see cref="UnauthorizedException"/>
/// when the caller is not authenticated at all.
/// </remarks>
/// <seealso cref="Controllers.ControllerUtils"/>
public class ForbiddenException : WebServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    public ForbiddenException() : base(403, "Forbidden")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ForbiddenException(string message) : base(403, message)
    {
    }
}
