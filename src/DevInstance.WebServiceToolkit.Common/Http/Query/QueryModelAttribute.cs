using System;

namespace DevInstance.WebServiceToolkit.Http.Query;

/// <summary>Marks a POCO as a query model to opt-in to helpers.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class QueryModelAttribute : Attribute 
{ 
}
