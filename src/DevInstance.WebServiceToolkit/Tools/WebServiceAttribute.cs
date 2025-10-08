namespace DevInstance.WebServiceToolkit.Tools;

/// <summary>
/// Specifies that a class represents a service, which abstracts and encapsulates business logic from the controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class WebServiceAttribute : Attribute
{

}
