using System;
using System.Collections.Generic;

namespace DevInstance.WebServiceToolkit.Http.Query;

public sealed class QueryModelBindException : Exception
{
    public IReadOnlyDictionary<string, string> Errors { get; }
    public QueryModelBindException(string message, IReadOnlyDictionary<string, string> errors)
        : base(message) => Errors = errors;
}
