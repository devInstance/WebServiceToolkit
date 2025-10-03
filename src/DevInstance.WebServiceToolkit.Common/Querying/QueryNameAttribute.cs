using System;

namespace NoCrast.Server.WebService.Common.Querying;

/// <summary>Overrides the query parameter name for a property.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class QueryNameAttribute : Attribute
{
    public QueryNameAttribute(string name) => Name = name;
    public string Name { get; }
}
