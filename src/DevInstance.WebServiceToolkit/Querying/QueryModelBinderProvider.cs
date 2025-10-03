using DevInstance.WebServiceToolkit.Common.Querying;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Reflection;

namespace DevInstance.WebServiceToolkit.Querying;

public sealed class QueryModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        var t = context.Metadata.ModelType;
        return t.GetCustomAttribute<QueryModelAttribute>() is not null
            ? new QueryModelMvcBinder()
            : null;
    }
}
