using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Linq;

namespace DevInstance.WebServiceToolkit.Querying;

public static class RegistrationExtensions
{
    /// <summary>
    /// Registers the [QueryModel] MVC model binder provider.
    /// Works regardless of when AddControllers() is called (order-independent).
    /// </summary>
    public static IServiceCollection AddWebServiceToolkitQuerying(this IServiceCollection services)
    {
        // Configure MvcOptions at the container level.
        services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, QueryModelMvcConfigureOptions>());
        return services;
    }

    /// <summary>
    /// Convenience overload to chain from AddControllers().
    /// </summary>
    public static IMvcBuilder AddWebServiceToolkitQuerying(this IMvcBuilder builder)
    {
        builder.Services.AddWebServiceToolkitQuerying();
        // Also allow opting-in via AddMvcOptions for explicitness if desired.
        builder.AddMvcOptions(o => o.UseWebServiceToolkitQuerying());
        return builder;
    }

    /// <summary>
    /// Low-level hook to insert the provider directly from MvcOptions.
    /// </summary>
    public static MvcOptions UseWebServiceToolkitQuerying(this MvcOptions options)
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
        public void Configure(MvcOptions options) => options.UseWebServiceToolkitQuerying();
    }
}
