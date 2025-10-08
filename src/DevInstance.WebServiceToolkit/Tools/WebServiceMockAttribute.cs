namespace DevInstance.WebServiceToolkit.Tools;

/// <summary>
/// Indicates that a class is used as a mock implementation of a service for testing or development purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class WebServiceMockAttribute : Attribute
{

}
