﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace DevInstance.WebServiceToolkit.Http.Query;

public sealed class QueryModelMvcBinder : IModelBinder
{
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
