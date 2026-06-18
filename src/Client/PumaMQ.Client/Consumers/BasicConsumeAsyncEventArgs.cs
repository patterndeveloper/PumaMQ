namespace PumaMQ.Client.Consumers;

public class BasicConsumeAsyncEventArgs : AsyncEventArgs
{
    public string ConsumerTag { get; }

    public BasicConsumeAsyncEventArgs(string consumerTag, CancellationToken cancellationToken) : base(cancellationToken)
    {
        ConsumerTag = consumerTag;
    }
}