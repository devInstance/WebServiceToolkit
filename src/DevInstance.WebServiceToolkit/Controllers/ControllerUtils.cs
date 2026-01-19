using DevInstance.WebServiceToolkit.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DevInstance.WebServiceToolkit.Controllers;

/// <summary>
/// Provides extension methods for ASP.NET Core controllers to handle common web request patterns.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a standardized way to handle exceptions in controller actions,
/// automatically converting domain exceptions to appropriate HTTP status codes:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="RecordNotFoundException"/> maps to HTTP 404 Not Found</description></item>
/// <item><description><see cref="RecordConflictException"/> maps to HTTP 409 Conflict</description></item>
/// <item><description><see cref="UnauthorizedException"/> maps to HTTP 401 Unauthorized</description></item>
/// <item><description><see cref="BadRequestException"/> maps to HTTP 400 Bad Request</description></item>
/// <item><description>Other exceptions map to HTTP 500 with problem details</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// [HttpGet("{id}")]
/// public Task&lt;ActionResult&lt;Product&gt;&gt; GetProduct(string id)
/// {
///     return this.HandleWebRequestAsync&lt;Product&gt;(async () =>
///     {
///         var product = await _productService.GetByIdAsync(id);
///         return Ok(product);
///     });
/// }
/// </code>
/// </example>
public static class ControllerUtils
{
    /// <summary>
    /// Delegate for synchronous web request handlers.
    /// </summary>
    /// <typeparam name="T">The type of the action result value.</typeparam>
    /// <returns>An action result containing a value of type <typeparamref name="T"/>.</returns>
    public delegate ActionResult<T> WebHandler<T>();

    /// <summary>
    /// Delegate for asynchronous web request handlers.
    /// </summary>
    /// <typeparam name="T">The type of the action result value.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing an action result with a value of type <typeparamref name="T"/>.</returns>
    public delegate Task<ActionResult<T>> WebHandlerAsync<T>();

    private static ActionResult<T> HandleException<T>(ControllerBase controller, Exception ex)
    {
        return controller.Problem(detail: ex.StackTrace, title: ex.Message);
    }

    /// <summary>
    /// Executes an asynchronous handler and converts domain exceptions to appropriate HTTP responses.
    /// </summary>
    /// <typeparam name="T">The type of the action result value.</typeparam>
    /// <param name="controller">The controller instance.</param>
    /// <param name="handler">The asynchronous handler to execute.</param>
    /// <returns>A task representing the asynchronous operation, containing the action result.</returns>
    /// <remarks>
    /// <para>
    /// This method wraps the handler execution in a try-catch block that converts
    /// domain exceptions to appropriate HTTP status codes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpPost]
    /// public Task&lt;ActionResult&lt;Product&gt;&gt; CreateProduct(CreateProductRequest request)
    /// {
    ///     return this.HandleWebRequestAsync&lt;Product&gt;(async () =>
    ///     {
    ///         var product = await _productService.CreateAsync(request);
    ///         return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    ///     });
    /// }
    /// </code>
    /// </example>
    public static async Task<ActionResult<T>> HandleWebRequestAsync<T>(this ControllerBase controller, WebHandlerAsync<T> handler)
    {
        try
        {
            return await handler();
        }
        catch (RecordNotFoundException)
        {
            return controller.NotFound();
        }
        catch (RecordConflictException)
        {
            return controller.Conflict();
        }
        catch (UnauthorizedException ex)
        {
            return controller.Unauthorized(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return controller.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException<T>(controller, ex);
        }
    }

    /// <summary>
    /// Executes a synchronous handler and converts domain exceptions to appropriate HTTP responses.
    /// </summary>
    /// <typeparam name="T">The type of the action result value.</typeparam>
    /// <param name="controller">The controller instance.</param>
    /// <param name="handler">The synchronous handler to execute.</param>
    /// <returns>The action result from the handler or an error response.</returns>
    /// <remarks>
    /// <para>
    /// This method wraps the handler execution in a try-catch block that converts
    /// domain exceptions to appropriate HTTP status codes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpGet("health")]
    /// public ActionResult&lt;HealthStatus&gt; GetHealth()
    /// {
    ///     return this.HandleWebRequest&lt;HealthStatus&gt;(() =>
    ///     {
    ///         return Ok(new HealthStatus { Status = "Healthy" });
    ///     });
    /// }
    /// </code>
    /// </example>
    public static ActionResult<T> HandleWebRequest<T>(this ControllerBase controller, WebHandler<T> handler)
    {
        try
        {
            return handler();
        }
        catch (RecordNotFoundException)
        {
            return controller.NotFound();
        }
        catch (RecordConflictException)
        {
            return controller.Conflict();
        }
        catch (UnauthorizedException ex)
        {
            return controller.Unauthorized(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return controller.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException<T>(controller, ex);
        }
    }
}
