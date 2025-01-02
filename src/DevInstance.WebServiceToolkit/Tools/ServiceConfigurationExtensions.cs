using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DevInstance.WebServiceToolkit.Tools;

public static class ServiceConfigurationExtensions
{
    public static void AddServerWebServices(this IServiceCollection services)
    {
        foreach (var type in GetTypesWithHelpAttribute(Assembly.GetCallingAssembly()))
        {
            services.AddScoped(type);
        }
    }

    static IEnumerable<Type> GetTypesWithHelpAttribute(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(WebServiceAttribute), true).Length > 0)
            {
                yield return type;
            }
        }
    }
}
