namespace DevInstance.WebServiceToolkit.Exceptions;

/// <summary>
/// Exception that represents an HTTP 409 Conflict response.
/// </summary>
/// <remarks>
/// <para>
/// Throw this exception from your service layer when a request conflicts with the current state
/// of the resource, such as attempting to create a duplicate record or violating a unique constraint.
/// When caught by <see cref="Controllers.ControllerUtils.HandleWebRequestAsync{T}"/>, it is automatically
/// converted to an HTTP 409 response.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public async Task&lt;User&gt; CreateUserAsync(CreateUserRequest request)
/// {
///     var existing = await _repository.FindByEmailAsync(request.Email);
///     if (existing != null)
///         throw new RecordConflictException($"User with email {request.Email} already exists");
///
///     // ... create logic
/// }
/// </code>
/// </example>
/// <seealso cref="Controllers.ControllerUtils"/>
public class RecordConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordConflictException"/> class.
    /// </summary>
    public RecordConflictException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordConflictException"/> class with a specified message.
    /// </summary>
    /// <param name="message">A description of the conflict.</param>
    public RecordConflictException(string message) : base($"Record conflict: {message}")
    {
    }
}
