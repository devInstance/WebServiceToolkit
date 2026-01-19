using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>
/// ASP.NET Core MVC model binder provider that creates <see cref="QueryModelMvcBinder"/> instances
/// for types marked with <see cref="QueryModelAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// This provider is automatically registered when you call
/// <see cref="RegistrationExtensions.AddWebServiceToolkitQuery(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
/// </para>
/// <para>
/// The provider checks if the target model type has the <see cref="QueryModelAttribute"/> applied
/// and returns a <see cref="QueryModelMvcBinder"/> if so, or null to let other binders handle the type.
/// </para>
/// </remarks>
public sealed class QueryModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// Gets a model binder for the specified context if the model type is marked with <see cref="QueryModelAttribute"/>.
    /// </summary>
    /// <param name="context">The model binder provider context.</param>
    /// <returns>A <see cref="QueryModelMvcBinder"/> if the model type has <see cref="QueryModelAttribute"/>; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        var t = context.Metadata.ModelType;
        return t.GetCustomAttribute<QueryModelAttribute>() is not null
            ? new QueryModelMvcBinder()
            : null;
    }
}
