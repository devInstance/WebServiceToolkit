using System;
using System.Collections.Generic;

namespace DevInstance.WebServiceToolkit.Querying;

public sealed class QueryModelBindException : Exception
{
    public IReadOnlyDictionary<string, string> Errors { get; }
    public QueryModelBindException(string message, IReadOnlyDictionary<string, string> errors)
        : base(message) => Errors = errors;
}
