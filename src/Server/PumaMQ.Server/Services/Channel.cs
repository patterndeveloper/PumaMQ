using PumaMQ.Server.Framings;
using PumaMQ.Server.Parsers;
using PumaMQ.Server.Rpcs;
using System.Text;

namespace PumaMQ.Server.Services;

internal class Channel
{
    internal ushort ChannelNo { get; }

    private SimpleRpcAwaitable? _rpcAwaitable;
    private readonly SemaphoreSlim _rpcSemaphore = new SemaphoreSlim(1, 1);

    private readonly L2Parser _l2Parser;
    private readonly SocketFrameHandler _socketFrameHandler;

    private readonly TimeSpan _rpcTimeout = TimeSpan.FromMinutes(1);

    internal Channel(SocketFrameHandler socketFrameHandler, ushort channelNo)
    {
        ChannelNo = channelNo;
        _socketFrameHandler = socketFrameHandler ?? throw new ArgumentNullException(nameof(socketFrameHandler));
        _l2Parser = new L2Parser();
    }


    internal async Task HandleL2FrameAsync(L1Frame l1Frame)
    {
        L2Frame? l2Frame = _l2Parser.Parse(l1Frame);

        if (l2Frame == null)
        {
            return;
        }

        if (l2Frame.ClassMethod == ClassMethod.ChannelOpen)
        {
            await HandleChannelOpenAsync(l2Frame);
        }

        string receivedMessage = Encoding.UTF8.GetString(l2Frame.Body.Memory.Span);

        Console.WriteLine($"Message: {receivedMessage} received from client");
    }


    internal async Task HandleChannelOpenAsync(L2Frame l2Frame)
    {
        await Task.Delay(60000);
        ChannelOpenOk channelOpenOk = new();
        RentedMemory serializedChannelOpenOk = FrameSerializer.Serialize(ref channelOpenOk, ChannelNo);
        await _socketFrameHandler.WriteAsync(serializedChannelOpenOk);
    }

    //Todo: Add heartbeat handling for channel 0
}