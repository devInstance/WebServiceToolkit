namespace DevInstance.WebServiceToolkit.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException()
    {
    }

    public UnauthorizedException(string message) : base($"Unauthorized: {message}")
    {
    }
}
