using PumaMQ.Client.Framings;
using PumaMQ.Client.Rpcs;

namespace PumaMQ.Client.Services;

public class Channel
{
    private readonly SocketFrameHandler _socketFrameHandler;

    public ushort ChannelNo { get; private set; }

    private RpcAwaitable<bool>? _rpcAwaitable;
    private readonly SemaphoreSlim _rpcSemaphore = new(1, 1);

    private readonly TimeSpan _rpcTimeout = TimeSpan.FromMinutes(1);


    private Channel(SocketFrameHandler socketFrameHandler, ushort channelNo)
    {
        ChannelNo = channelNo;
        _socketFrameHandler = socketFrameHandler ?? throw new ArgumentNullException(nameof(socketFrameHandler));
    }


    internal static Channel Create(SocketFrameHandler socketFrameHandler, ushort channelNo)
    {
        Channel channel = new Channel(socketFrameHandler, channelNo);
        return channel;
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


    public async Task ChannelOpenAsync(CancellationToken cancellationToken = default)
    {
        _rpcAwaitable = new SimpleRpcAwaitable(ClassMethod.ChannelOpenOk, _rpcTimeout, cancellationToken);
        await _rpcSemaphore.WaitAsync(_rpcAwaitable.LinkedCancellationToken);

        try
        {
            ChannelOpen channelOpen = new ChannelOpen();
            RentedMemory serializedChannelOpen = FrameSerializer.Serialize(ref channelOpen, channelNo: 1);
            await _socketFrameHandler.WriteAsync(serializedChannelOpen);

            await _rpcAwaitable;
        }
        finally
        {
            _rpcSemaphore.Release();
            _rpcAwaitable?.Dispose();
        }
    }
}
