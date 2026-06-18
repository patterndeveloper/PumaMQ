using PumaMQ.Client.Consumers;
using PumaMQ.Client.Parsers;
using PumaMQ.Client.Rpcs;
using System.Collections.Concurrent;

namespace PumaMQ.Client.Services;

public partial class Channel
{
    private readonly L2Parser _l2Parser;
    private readonly SocketFrameHandler _socketFrameHandler;

    public ushort ChannelNo { get; private set; }

    private readonly ConcurrentDictionary<string, Consumer> _consumers = [];

    private IRpcAwaitable? _rpcAwaitable;
    private readonly SemaphoreSlim _rpcSemaphore = new(1, 1);

    private readonly TimeSpan _rpcTimeout = TimeSpan.FromMinutes(10);


    private Channel(SocketFrameHandler socketFrameHandler, ushort channelNo)
    {
        ChannelNo = channelNo;
        _l2Parser = new L2Parser();
        _socketFrameHandler = socketFrameHandler ?? throw new ArgumentNullException(nameof(socketFrameHandler));
    }


    internal static Channel Create(SocketFrameHandler socketFrameHandler, ushort channelNo)
    {
        Channel channel = new Channel(socketFrameHandler, channelNo);
        return channel;
    }
}
