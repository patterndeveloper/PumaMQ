namespace PumaMQ.Client.Exceptions;

public class PumaMQException : Exception
{
    public PumaMQException(string message, Exception? innerException) : base(message, innerException)
    { }
}