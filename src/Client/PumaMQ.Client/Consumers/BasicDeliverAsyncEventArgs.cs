namespace PumaMQ.Client.Consumers;

public class BasicDeliverAsyncEventArgs : AsyncEventArgs
{
    public BasicDeliverAsyncEventArgs(CancellationToken cancellationToken = default) : base(cancellationToken)
    {
    }
}