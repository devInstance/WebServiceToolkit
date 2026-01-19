namespace DevInstance.WebServiceToolkit.Exceptions;

/// <summary>
/// Exception that represents an HTTP 401 Unauthorized response.
/// </summary>
/// <remarks>
/// <para>
/// Throw this exception from your service layer when authentication fails or the user
/// is not authenticated. When caught by <see cref="Controllers.ControllerUtils.HandleWebRequestAsync{T}"/>,
/// it is automatically converted to an HTTP 401 response with the exception message.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public async Task&lt;User&gt; GetCurrentUserAsync(string token)
/// {
///     var user = await _authService.ValidateTokenAsync(token);
///     if (user == null)
///         throw new UnauthorizedException("Invalid or expired token");
///
///     return user;
/// }
/// </code>
/// </example>
/// <seealso cref="Controllers.ControllerUtils"/>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    public UnauthorizedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a specified message.
    /// </summary>
    /// <param name="message">A description of why authentication failed.</param>
    public UnauthorizedException(string message) : base($"Unauthorized: {message}")
    {
    }
}
