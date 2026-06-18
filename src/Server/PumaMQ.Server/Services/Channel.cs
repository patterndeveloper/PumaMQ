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

        if(l2Frame.ClassMethod == ClassMethod.BasicConsume)
        {
            await HandleBasicConsumeAsync(l2Frame);
        }

        string receivedMessage = Encoding.UTF8.GetString(l2Frame.Body.Memory.Span);

        Console.WriteLine($"Message: {receivedMessage} received from client");
    }


    internal async Task HandleChannelOpenAsync(L2Frame l2Frame)
    {
        ChannelOpenOk channelOpenOk = new();
        RentedMemory serializedChannelOpenOk = FrameSerializer.Serialize(ref channelOpenOk, ChannelNo);
        await _socketFrameHandler.WriteAsync(serializedChannelOpenOk).ConfigureAwait(false);
    }


    internal async Task HandleBasicConsumeAsync(L2Frame l2Frame)
    {
        //1- Parse payload of method frame
        BasicConsume basicConsume = new BasicConsume(l2Frame);

        //2- Generate consumer-tag
        string consumerTag = "consumerTag-1";

        //3- Insert consumer (consumerTag, queueId, channelId) into db

        //4- Return Basic.Consume-Ok
        BasicConsumeOk basicConsumeOk = new BasicConsumeOk(consumerTag);
        RentedMemory serializedBasicConsumeOk = FrameSerializer.Serialize(ref basicConsumeOk, ChannelNo);
        await _socketFrameHandler.WriteAsync(serializedBasicConsumeOk).ConfigureAwait(false);
    }

    //Todo: Add heartbeat handling for channel 0
}