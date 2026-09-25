namespace DevInstance.WebServiceToolkit.Exceptions;

/// <summary>
/// Exception that represents an HTTP 422 Unprocessable Entity response.
/// </summary>
/// <remarks>
/// Throw this (or derive a domain exception from it) when a request is well-formed but violates a
/// business rule — e.g. "an organization cannot be moved under its own descendant".
/// Use <see cref="BadRequestException"/> for malformed input.
/// </remarks>
/// <seealso cref="Controllers.ControllerUtils"/>
public class UnprocessableEntityException : WebServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnprocessableEntityException"/> class.
    /// </summary>
    public UnprocessableEntityException() : base(422)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnprocessableEntityException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="propertyName">Optional name of the request property that violates the rule.</param>
    public UnprocessableEntityException(string message, string? propertyName = null) : base(422, message, propertyName)
    {
    }
}
