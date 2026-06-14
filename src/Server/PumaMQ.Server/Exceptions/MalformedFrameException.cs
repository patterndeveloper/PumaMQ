namespace PumaMQ.Server.Exceptions;

public class MalformedFrameException : PumaMQException
{
    public MalformedFrameException(string message, Exception? innerException = default) : base(message, innerException)
    {
    }
}