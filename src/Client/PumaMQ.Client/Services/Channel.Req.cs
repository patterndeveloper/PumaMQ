using PumaMQ.Client.Consumers;
using PumaMQ.Client.Framings;
using PumaMQ.Client.Rpcs;

namespace PumaMQ.Client.Services;

public partial class Channel
{
    public async Task BasicConsumeAsync(string queue, Consumer consumer, CancellationToken cancellationToken = default)
    {
        //1-Create rpc awatable
        BasicConsumeRpcAwaitable rpcAwaitable = new BasicConsumeRpcAwaitable(ClassMethod.BasicConsumeOk, consumer, _rpcTimeout, cancellationToken);
        await _rpcSemaphore.WaitAsync(rpcAwaitable.LinkedCancellationToken).ConfigureAwait(false);
        _rpcAwaitable = rpcAwaitable;

        try
        {
            //2- send basic consume frame
            BasicConsume basicConsume = new BasicConsume(queue);
            RentedMemory serializedBasicConsume = FrameSerializer.Serialize(ref basicConsume, ChannelNo);
            await _socketFrameHandler.WriteAsync(serializedBasicConsume, rpcAwaitable.LinkedCancellationToken).ConfigureAwait(false);

            //3- await rpc
            await rpcAwaitable;

            //4- call consumer raiseEvent (Done in awaitable.Complete())
        }
        finally
        {
            _rpcSemaphore.Release();
            rpcAwaitable?.Dispose();
        }
    }


    public async Task ChannelOpenAsync(CancellationToken cancellationToken = default)
    {
        SimpleRpcAwaitable rpcAwaitable = new SimpleRpcAwaitable(ClassMethod.ChannelOpenOk, _rpcTimeout, cancellationToken);
        await _rpcSemaphore.WaitAsync(rpcAwaitable.LinkedCancellationToken).ConfigureAwait(false);
        _rpcAwaitable = rpcAwaitable;

        try
        {
            ChannelOpen channelOpen = new ChannelOpen();
            RentedMemory serializedChannelOpen = FrameSerializer.Serialize(ref channelOpen, ChannelNo);
            await _socketFrameHandler.WriteAsync(serializedChannelOpen, rpcAwaitable.LinkedCancellationToken).ConfigureAwait(false);

            await rpcAwaitable;
        }
        finally
        {
            _rpcSemaphore.Release();
            rpcAwaitable?.Dispose();
        }
    }


    //Todo: add basic property
    public async Task BasicPublishAsync(string exchange,
                                        string routingKey,
                                        bool mandatory,
                                        ReadOnlyMemory<byte> body,
                                        CancellationToken cancellationToken = default)
    {
        BasicPublish basicPublish = new BasicPublish(exchange, routingKey, mandatory, immediate: false);

        RentedMemory serializedBasicPublish = FrameSerializer.Serialize(ref basicPublish, body, channelNo: 1);

        await _socketFrameHandler.WriteAsync(serializedBasicPublish);
    }
}
