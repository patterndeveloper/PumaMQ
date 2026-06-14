namespace PumaMQ.Client.Exceptions;

public class StreamFinishedException : PumaMQException
{
    public StreamFinishedException(string message, Exception? innerException = default) : base(message, innerException)
    {
    }
}