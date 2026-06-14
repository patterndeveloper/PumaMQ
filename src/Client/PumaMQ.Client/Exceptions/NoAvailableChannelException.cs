namespace PumaMQ.Client.Exceptions;

public class NoAvailableChannelException : PumaMQException
{
    public NoAvailableChannelException(string message, Exception? innerException = default) : base(message, innerException)
    {
    }
}