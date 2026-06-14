namespace PumaMQ.Server.Exceptions;

public class UnKnownClassOrMethodException : PumaMQException
{
    public UnKnownClassOrMethodException(string message, Exception? innerException = default) : base(message, innerException)
    {
    }
}