namespace PumaMQ.Server.Exceptions;

public class ConnectionFailureException : PumaMQException
{
    public ConnectionFailureException(string message, Exception innerException) : base(message, innerException)
    {
    }
}