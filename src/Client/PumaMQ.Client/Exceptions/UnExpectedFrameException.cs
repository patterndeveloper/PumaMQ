namespace PumaMQ.Client.Exceptions;

public class UnExpectedFrameException : PumaMQException
{
    public UnExpectedFrameException(string message, Exception? innerException = default) : base(message, innerException)
    {
    }
}
