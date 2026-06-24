using Microsoft.Extensions.DependencyInjection;
using PumaMQ.Server.Parsers;
using PumaMQ.Server.Persistences;
using PumaMQ.Server.Rpcs;

namespace PumaMQ.Server.Services;

internal partial class Channel
{
    internal int Id { get; set; }
    internal ushort ChannelNo { get; }

    private readonly IServiceProvider _serviceProvider;
    private readonly ChannelRepository _channelRepository;
    private readonly ConsumerRepository _consumerRepository;
    private readonly ExchangeRepository _exchangeRepository;
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
                     ExchangeRepository exchangeRepository,
                     QueueRepository queueRepository,
                     SocketFrameHandler socketFrameHandler,
                     ushort channelNo)
    {
        Id = id;
        ChannelNo = channelNo;

        _serviceProvider = serviceProvider;
        _channelRepository = channelRepository;
        _consumerRepository = consumerRepository;
        _exchangeRepository = exchangeRepository;
        _queueRepository = queueRepository;

        _socketFrameHandler = socketFrameHandler;
        _l2Parser = new();
    }


    internal static async Task<Channel> CreateAsync(IServiceProvider serviceProvider,
                                                    SocketFrameHandler socketFrameHandler,
                                                    ushort channelNo,
                                                    int connectionId)
    {
        ChannelRepository channelRepository = serviceProvider.GetRequiredService<ChannelRepository>();
        ConsumerRepository consumerRepository = serviceProvider.GetRequiredService<ConsumerRepository>();
        ExchangeRepository exchangeRepository = serviceProvider.GetRequiredService<ExchangeRepository>();
        QueueRepository queueRepository = serviceProvider.GetRequiredService<QueueRepository>();

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

        Channel channel = new(id, serviceProvider, channelRepository, consumerRepository, exchangeRepository, queueRepository, socketFrameHandler, channelNo);
        return channel;
    }


    //Todo: Add heartbeat handling for channel 0
}