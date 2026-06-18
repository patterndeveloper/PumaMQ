namespace PumaMQ.Client.Consumers;

public class AsyncEventArgs
{
    public CancellationToken CancellationToken { get; }

    public AsyncEventArgs(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
    }
}