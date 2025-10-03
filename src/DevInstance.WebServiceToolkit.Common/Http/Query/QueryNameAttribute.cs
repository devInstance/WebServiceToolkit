using System;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>Overrides the query parameter name for a property.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class QueryNameAttribute : Attribute
{
    public QueryNameAttribute(string name) => Name = name;
    public string Name { get; }
}
