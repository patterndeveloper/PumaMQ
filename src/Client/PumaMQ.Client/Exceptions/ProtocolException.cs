namespace PumaMQ.Client.Exceptions;

public class ProtocolException : PumaMQException
{
    public ProtocolException(string message, Exception? innerException = default) : base(message, innerException)
    {
    }
}
