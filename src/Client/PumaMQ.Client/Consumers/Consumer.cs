namespace PumaMQ.Client.Consumers;

public class Consumer
{
    public string Tag { get; set; }

    public event AsyncEventHandler<BasicConsumeAsyncEventArgs>? BasicConsumed;
    public event AsyncEventHandler<BasicDeliverAsyncEventArgs>? BasicDelivered;

    public Consumer()
    {
        Tag = string.Empty;
    }


    public virtual async Task HandleBasicConsumeOk(string consumerTag, CancellationToken cancellationToken = default)
    {
        BasicConsumeAsyncEventArgs eventArgs = new BasicConsumeAsyncEventArgs(consumerTag, cancellationToken);

        if(BasicConsumed != null)
        {
           await BasicConsumed(this, eventArgs);
        }
    }


    public virtual async Task HandleBasicDelivered(string consumerTag,
                                                   ulong deliveryTag,
                                                   bool redelivered,
                                                   string exchange,
                                                   string routingKey,
                                                   ReadOnlyMemory<byte> body,
                                                   CancellationToken cancellationToken = default)
    {
        BasicDeliverAsyncEventArgs eventArgs = new BasicDeliverAsyncEventArgs();

        if(BasicDelivered != null)
        {
            await BasicDelivered(this, eventArgs);
        }
    }
}