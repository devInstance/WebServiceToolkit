using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DevInstance.WebServiceToolkit.Controllers;

/// <summary>
/// Registers the <see cref="WebServiceError"/> response shape for errors that ASP.NET Core
/// produces before a controller action runs.
/// </summary>
/// <remarks>
/// <para>
/// <c>[ApiController]</c> answers an invalid model state (failed data annotations, malformed JSON,
/// <c>[QueryModel]</c> binding errors) with a <c>ValidationProblemDetails</c> body. Clients that
/// expect a <see cref="WebServiceError"/> cannot read it. This replaces that response with a
/// 400 and a <see cref="WebServiceError"/> of type <see cref="WebServiceErrorType.Validation"/>
/// naming the first invalid property.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// builder.Services.AddControllers()
///     .AddWebServiceToolkitQuery()
///     .AddWebServiceToolkitErrors();
/// </code>
/// </example>
public static class ErrorRegistrationExtensions
{
    /// <summary>
    /// Makes automatic model-validation 400 responses use the <see cref="WebServiceError"/> body.
    /// </summary>
    public static IMvcBuilder AddWebServiceToolkitErrors(this IMvcBuilder builder)
    {
        builder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var first = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new { Property = e.Key, e.Value!.Errors[0].ErrorMessage })
                    .FirstOrDefault();

                var error = new WebServiceError
                {
                    ErrorType = WebServiceErrorType.Validation,
                    Message = string.IsNullOrEmpty(first?.ErrorMessage) ? "The request is invalid." : first.ErrorMessage,
                    PropertyName = string.IsNullOrEmpty(first?.Property) ? null : first.Property
                };

                return new BadRequestObjectResult(error);
            };
        });
        return builder;
    }
}
