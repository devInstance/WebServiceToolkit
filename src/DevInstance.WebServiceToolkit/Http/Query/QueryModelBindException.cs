using System;
using System.Collections.Generic;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>
/// Exception thrown when query model binding fails due to invalid parameter values.
/// </summary>
/// <remarks>
/// <para>
/// This exception contains a dictionary of field-specific errors that can be used
/// to provide detailed validation feedback to the client.
/// </para>
/// <para>
/// When using the MVC model binder (<see cref="QueryModelMvcBinder"/>), these errors
/// are automatically added to the ModelState rather than throwing this exception.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     var query = QueryModelBinder.Bind&lt;ProductQuery&gt;(request);
/// }
/// catch (QueryModelBindException ex)
/// {
///     foreach (var error in ex.Errors)
///     {
///         Console.WriteLine($"Parameter '{error.Key}': {error.Value}");
///     }
/// }
/// </code>
/// </example>
public sealed class QueryModelBindException : Exception
{
    /// <summary>
    /// Gets a dictionary of parameter names to their corresponding error messages.
    /// </summary>
    /// <value>A read-only dictionary where keys are parameter names and values are error descriptions.</value>
    public IReadOnlyDictionary<string, string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryModelBindException"/> class.
    /// </summary>
    /// <param name="message">The error message that describes the binding failure.</param>
    /// <param name="errors">A dictionary of parameter names to their error messages.</param>
    public QueryModelBindException(string message, IReadOnlyDictionary<string, string> errors)
        : base(message) => Errors = errors;
}
