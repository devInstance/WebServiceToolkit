using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>
/// Provides extension methods for registering query model binding services in the dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// Use these extension methods in your application startup to enable automatic binding
/// of query string parameters to classes marked with <see cref="QueryModelAttribute"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In Program.cs
/// var builder = WebApplication.CreateBuilder(args);
///
/// // Option 1: Chain from AddControllers
/// builder.Services.AddControllers()
///     .AddWebServiceToolkitQuery();
///
/// // Option 2: Separate registration
/// builder.Services.AddControllers();
/// builder.Services.AddWebServiceToolkitQuery();
/// </code>
/// </example>
public static class RegistrationExtensions
{
    /// <summary>
    /// Registers the [QueryModel] MVC model binder provider.
    /// Works regardless of when AddControllers() is called (order-independent).
    /// </summary>
    public static IServiceCollection AddWebServiceToolkitQuery(this IServiceCollection services)
    {
        // Configure MvcOptions at the container level.
        services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, QueryModelMvcConfigureOptions>());
        return services;
    }

    /// <summary>
    /// Convenience overload to chain from AddControllers().
    /// </summary>
    public static IMvcBuilder AddWebServiceToolkitQuery(this IMvcBuilder builder)
    {
        builder.Services.AddWebServiceToolkitQuery();
        // Also allow opting-in via AddMvcOptions for explicitness if desired.
        builder.AddMvcOptions(o => o.UseWebServiceToolkitQuery());
        return builder;
    }

    /// <summary>
    /// Low-level hook to insert the provider directly from MvcOptions.
    /// </summary>
    public static MvcOptions UseWebServiceToolkitQuery(this MvcOptions options)
    {
        if (!options.ModelBinderProviders.Any(p => p is QueryModelBinderProvider))
        {
            // Insert early so it runs before the default FromQuery binder.
            options.ModelBinderProviders.Insert(0, new QueryModelBinderProvider());
        }
        return options;
    }

    // Internal: options configurator that safely inserts the provider once.
    private sealed class QueryModelMvcConfigureOptions : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options) => options.UseWebServiceToolkitQuery();
    }
}
