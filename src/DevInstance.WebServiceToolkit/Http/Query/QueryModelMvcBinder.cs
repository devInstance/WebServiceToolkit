using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>
/// ASP.NET Core MVC model binder that binds query string parameters to classes marked with <see cref="QueryModelAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// This binder uses <see cref="QueryModelBinder.TryBind{T}"/> internally and adds any binding errors
/// to the MVC ModelState for validation handling.
/// </para>
/// <para>
/// This binder is automatically used when <see cref="QueryModelBinderProvider"/> is registered
/// via <see cref="RegistrationExtensions.AddWebServiceToolkitQuery(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
/// </para>
/// </remarks>
public sealed class QueryModelMvcBinder : IModelBinder
{
    /// <summary>
    /// Attempts to bind query string parameters to the model.
    /// </summary>
    /// <param name="ctx">The model binding context.</param>
    /// <returns>A completed task representing the asynchronous binding operation.</returns>
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        var t = ctx.ModelType;
        if (t.GetCustomAttribute<QueryModelAttribute>() is null)
        {
            ctx.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // Call generic TryBind<T> via reflection
        var tryBind = typeof(QueryModelBinder).GetMethod(nameof(QueryModelBinder.TryBind))!
                                              .MakeGenericMethod(t);

        var args = new object?[] { ctx.HttpContext.Request, null, null! };
        var success = (bool)tryBind.Invoke(null, args)!;

        if (success)
        {
            ctx.Result = ModelBindingResult.Success(args[1]);
        }
        else
        {
            var errors = (IReadOnlyDictionary<string, string>)args[2]!;
            foreach (var kv in errors)
                ctx.ModelState.AddModelError(kv.Key, kv.Value);

            // Provide the (partially) bound model so defaults are preserved
            ctx.Result = ModelBindingResult.Success(args[1]);
        }

        return Task.CompletedTask;
    }
}
