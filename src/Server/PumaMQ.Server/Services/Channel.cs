using Microsoft.Extensions.DependencyInjection;
using PumaMQ.Server.Framings;
using PumaMQ.Server.Models;
using PumaMQ.Server.Parsers;
using PumaMQ.Server.Persistences;
using PumaMQ.Server.Rpcs;
using System.Text;

namespace PumaMQ.Server.Services;

internal class Channel
{
    internal int Id { get; set; }
    internal ushort ChannelNo { get; }

    private readonly IServiceProvider _serviceProvider;
    private readonly ChannelRepository _channelRepository;
    private readonly ConsumerRepository _consumerRepository;
    private readonly QueueRepository _queueRepository;

    private readonly SocketFrameHandler _socketFrameHandler;
    private readonly L2Parser _l2Parser;

    private SimpleRpcAwaitable? _rpcAwaitable;
    private readonly SemaphoreSlim _rpcSemaphore = new SemaphoreSlim(1, 1);
    private readonly TimeSpan _rpcTimeout = TimeSpan.FromMinutes(1);


    internal Channel(int id,
                     IServiceProvider serviceProvider,
                     ChannelRepository channelRepository,
                     ConsumerRepository consumerRepository,
                     SocketFrameHandler socketFrameHandler,
                     ushort channelNo)
    {
        Id = id;
        _serviceProvider = serviceProvider;
        _channelRepository = channelRepository;
        _consumerRepository = consumerRepository;
        _socketFrameHandler = socketFrameHandler;
        ChannelNo = channelNo;
        _l2Parser = new();
    }


    internal static async Task<Channel> CreateAsync(IServiceProvider serviceProvider,
                                                    SocketFrameHandler socketFrameHandler,
                                                    ushort channelNo,
                                                    int connectionId)
    {
        ChannelRepository channelRepository = serviceProvider.GetRequiredService<ChannelRepository>();
        ConsumerRepository consumerRepository = serviceProvider.GetRequiredService<ConsumerRepository>();

        Models.Channel storedChannel = new()
        {
            ChannelNo = channelNo,
            ConnectionId = connectionId
        };

        int id = 0;
        try
        {
            id = await channelRepository.CreateAsync(storedChannel).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            Exception exc = ex;
        }

        Channel channel = new(id, serviceProvider, channelRepository, consumerRepository, socketFrameHandler, channelNo);
        return channel;
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

        if (l2Frame.ClassMethod == ClassMethod.BasicConsume)
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

        //2- Insert consumer (consumerTag, queueId, channelId) into db
        Queue? queue = await _queueRepository.GetAsync(basicConsume.Queue).ConfigureAwait(false);

        if (queue == null)
        {
            //Todo: Add Queue not found exception
            throw new Exception("Queue not exist");
        }

        try
        {
            Consumer consumer = new()
            {
                QueueId = queue.Id,
                ChannelId = Id,
                Tag = basicConsume.Tag,
            };
            await _consumerRepository.CreateAsync(consumer);
        }
        catch (Exception ex)
        {
            throw;
        }

        //3- Return Basic.Consume-Ok
        BasicConsumeOk basicConsumeOk = new BasicConsumeOk(basicConsume.Tag);
        RentedMemory serializedBasicConsumeOk = FrameSerializer.Serialize(ref basicConsumeOk, ChannelNo);
        await _socketFrameHandler.WriteAsync(serializedBasicConsumeOk).ConfigureAwait(false);
    }

    //Todo: Add heartbeat handling for channel 0
}