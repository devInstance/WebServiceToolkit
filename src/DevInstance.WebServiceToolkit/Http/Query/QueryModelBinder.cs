using Microsoft.AspNetCore.Http;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>
/// Provides methods to bind query string parameters to classes marked with <see cref="QueryModelAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class is the core engine for parsing HTTP query strings into strongly-typed POCO objects.
/// It supports various data types including primitives, dates, enums, and collections.
/// </para>
/// <para>
/// For ASP.NET Core MVC integration, use <see cref="RegistrationExtensions.AddWebServiceToolkitQuery(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>
/// to register automatic model binding.
/// </para>
/// </remarks>
public static class QueryModelBinder
{
    /// <summary>
    /// Binds query string parameters from an HTTP request to a new instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The query model type marked with <see cref="QueryModelAttribute"/>.</typeparam>
    /// <param name="request">The HTTP request containing query parameters.</param>
    /// <returns>A new instance of <typeparamref name="T"/> populated with values from the query string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> is not marked with <see cref="QueryModelAttribute"/>.</exception>
    /// <exception cref="QueryModelBindException">Thrown when one or more parameters fail to bind.</exception>
    /// <example>
    /// <code>
    /// var query = QueryModelBinder.Bind&lt;ProductQuery&gt;(HttpContext.Request);
    /// </code>
    /// </example>
    public static T Bind<T>(HttpRequest request) where T : new()
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (!IsQueryModel(typeof(T)))
            throw new InvalidOperationException($"{typeof(T).Name} must be marked with [QueryModel].");

        if (!TryBind(request, out T model, out var errors))
            throw new QueryModelBindException("Invalid query parameters.", errors);

        return model;
    }

    /// <summary>
    /// Attempts to bind query string parameters from an HTTP request to a new instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The query model type marked with <see cref="QueryModelAttribute"/>.</typeparam>
    /// <param name="request">The HTTP request containing query parameters.</param>
    /// <param name="model">When this method returns, contains the populated model instance.</param>
    /// <param name="errors">When this method returns false, contains a dictionary of parameter names to error messages.</param>
    /// <returns><c>true</c> if binding was successful; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (QueryModelBinder.TryBind&lt;ProductQuery&gt;(request, out var query, out var errors))
    /// {
    ///     // Use query
    /// }
    /// else
    /// {
    ///     // Handle errors
    ///     foreach (var error in errors)
    ///         Console.WriteLine($"{error.Key}: {error.Value}");
    /// }
    /// </code>
    /// </example>
    public static bool TryBind<T>(HttpRequest request, out T model, out IReadOnlyDictionary<string, string> errors)
        where T : new()
    {
        model = new T();
        var err = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var t = typeof(T);
        if (!IsQueryModel(t))
        {
            errors = new Dictionary<string, string> { ["Model"] = "Type is not marked with [QueryModel]." };
            return false;
        }

        var q = request.Query;

        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.SetMethod is null) continue;

            var qName = p.GetCustomAttribute<QueryNameAttribute>()?.Name ?? p.Name;
            var hasValue = q.TryGetValue(qName, out var values);
            var raw = hasValue ? values.ToString() : null;

            if (!hasValue || string.IsNullOrWhiteSpace(raw))
            {
                // Apply [DefaultValue] if declared.
                var def = p.GetCustomAttribute<DefaultValueAttribute>();
                if (def is not null) p.SetValue(model, def.Value);
                continue;
            }

            try
            {
                object? converted;

                // Handle IEnumerable/arrays via comma-separated values
                if (TryGetEnumerableElementType(p.PropertyType, out var elemType, out var assignEnumerable))
                {
                    var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType))!;
                    foreach (var part in parts)
                    {
                        var parsed = ConvertOne(part, elemType, qName);
                        list.Add(parsed);
                    }
                    converted = assignEnumerable(list, p.PropertyType);
                }
                else
                {
                    converted = ConvertOne(raw, p.PropertyType, qName);
                }

                p.SetValue(model, converted);
            }
            catch (FormatException fe)
            {
                err[qName] = fe.Message;
            }
            catch (Exception ex)
            {
                err[qName] = $"Invalid value: {ex.Message}";
            }
        }

        errors = err;
        return err.Count == 0;
    }

    private static bool IsQueryModel(Type t)
        => t.GetCustomAttribute<QueryModelAttribute>() is not null;

    private static bool TryGetEnumerableElementType(Type type, out Type elementType,
        out Func<IList, Type, object> assignEnumerable)
    {
        assignEnumerable = null!;
        elementType = null!;
        if (type.IsArray)
        {
            var element = type.GetElementType()!;
            elementType = element;
            assignEnumerable = (list, _) =>
            {
                var arr = Array.CreateInstance(element, list.Count);
                list.CopyTo(arr, 0);
                return arr;
            };
            return true;
        }

        // IEnumerable<T> (e.g., List<T>, IEnumerable<T>, ICollection<T>)
        var ienum = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (ienum is null) return false;

        var genericElement = ienum.GetGenericArguments()[0];
        elementType = genericElement;
        assignEnumerable = (list, targetType) =>
        {
            // If property is List<T> or ICollection<T>, try to construct it; else return array
            if (!targetType.IsInterface && targetType.IsGenericType)
            {
                var instance = Activator.CreateInstance(targetType)!;
                var add = targetType.GetMethod("Add");
                foreach (var item in list) add!.Invoke(instance, new[] { item });
                return instance;
            }
            // fallback: array
            var arr = Array.CreateInstance(genericElement, list.Count);
            list.CopyTo(arr, 0);
            return arr;
        };
        return true;
    }

    private static object? ConvertOne(string raw, Type targetType, string param)
    {
        var (isNullable, nnType) = UnwrapNullable(targetType);

        if (nnType == typeof(string)) return raw;
        if (nnType == typeof(bool))
        {
            if (!bool.TryParse(raw, out var b))
                throw new FormatException("Expected boolean.");
            return b;
        }
        if (nnType == typeof(int))
        {
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                throw new FormatException("Expected integer.");
            return i;
        }
        if (nnType == typeof(long))
        {
            if (!long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                throw new FormatException("Expected long.");
            return l;
        }
        if (nnType == typeof(decimal))
        {
            if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
                throw new FormatException("Expected decimal.");
            return d;
        }
        if (nnType == typeof(double))
        {
            if (!double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var db))
                throw new FormatException("Expected double.");
            return db;
        }
        if (nnType == typeof(Guid))
        {
            if (!Guid.TryParse(raw, out var g))
                throw new FormatException("Expected GUID.");
            return g;
        }
        if (nnType == typeof(DateTime))
        {
            if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                throw new FormatException("Expected ISO 8601 DateTime.");
            return dt;
        }
#if NET7_0_OR_GREATER
        if (nnType == typeof(DateOnly))
        {
            if (!DateOnly.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                throw new FormatException("Expected yyyy-MM-dd.");
            return d;
        }
        if (nnType == typeof(TimeOnly))
        {
            // allow HH:mm or HH:mm:ss
            if (TimeOnly.TryParseExact(raw, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ||
                TimeOnly.TryParseExact(raw, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out t))
                return t;
            throw new FormatException("Expected HH:mm or HH:mm:ss.");
        }
#endif
        if (nnType.IsEnum)
        {
            if (!Enum.TryParse(nnType, raw, ignoreCase: true, out var e))
                throw new FormatException($"Expected one of: {string.Join(',', Enum.GetNames(nnType))}.");
            return e;
        }

        // Last resort: TypeConverter (e.g., custom types)
        var conv = TypeDescriptor.GetConverter(nnType);
        if (conv is not null && conv.CanConvertFrom(typeof(string)))
        {
            try { return conv.ConvertFromInvariantString(raw); }
            catch { throw new FormatException($"Invalid {nnType.Name}."); }
        }

        // Unhandled type; treat as string
        return raw;
    }

    private static (bool isNullable, Type nnType) UnwrapNullable(Type t)
        => (Nullable.GetUnderlyingType(t) is Type inner) ? (true, inner) : (false, t);
}
