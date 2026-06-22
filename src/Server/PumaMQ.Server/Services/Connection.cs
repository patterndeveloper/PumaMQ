using Microsoft.Extensions.DependencyInjection;
using PumaMQ.Server.Framings;
using PumaMQ.Server.Persistences;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace PumaMQ.Server.Services;

internal class Connection
{
    internal int Id { get; set; }

    private readonly IServiceProvider _serviceProvider;
    private readonly ConnectionRepository _connectionRepository;
    private readonly SocketFrameHandler _socketFrameHandler;

    private readonly Dictionary<ushort, Channel> _channels = [];


    public Connection(int id,
                      IServiceProvider serviceProvider,
                      ConnectionRepository connectionRepository,
                      TcpClient tcpClient)
    {
        _serviceProvider = serviceProvider;
        Id = id;
        _connectionRepository = connectionRepository;

        _socketFrameHandler = SocketFrameHandler.Create(tcpClient);

        Channel channel0 = Channel.CreateAsync(_serviceProvider, _socketFrameHandler, 0, Id).Result;
        _channels.Add(0, channel0);

        CancellationTokenSource readLoopCts = new CancellationTokenSource();
        Task readLoopTask = Task.Run(() => ReadLoopAsync(readLoopCts.Token));
    }


    internal static async Task<Connection> CreateAsync(IServiceProvider serviceProvider, TcpClient tcpClient)
    {
        ConnectionRepository connectionRepository = serviceProvider.GetRequiredService<ConnectionRepository>();

        Models.Connection storedConnection = new Models.Connection()
        {
            VirtualHost = "/",
            ConnectedAt = DateTime.UtcNow,
        };

        int id = await connectionRepository.CreateAsync(storedConnection);

        Connection connection = new Connection(id, serviceProvider, connectionRepository, tcpClient);
        return connection;
    }


    internal async Task ReadLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            while (true)
            {
                L1Frame l1Frame = await _socketFrameHandler.ReadL1FrameAsync();

                await ProcessL1Frame(l1Frame).ConfigureAwait(false);

                l1Frame.Payload.Dispose();
            }
        }
        catch (Exception ex)
        {
            //close connection and create new one
        }
    }


    private async Task ProcessL1Frame(L1Frame l1Frame)
    {
        if (l1Frame.Channel == 0)
        {
            if (l1Frame.Type == FrameType.HeartBeat)
            {
                //process read Heartbeat frame
                Console.WriteLine("A heart beat from received from consumer");
            }
        }
        else
        {
            Channel? channel = default;
            bool channelExist = _channels.TryGetValue(l1Frame.Channel, out channel);

            if (!channelExist)
            {
                //1- if frame is channel.open create channel
                channel = Channel.CreateAsync(_serviceProvider, _socketFrameHandler, l1Frame.Channel, Id).Result;
                _channels.Add(channel.ChannelNo, channel);

                //2- else throw exception and close connection
            }

            await channel!.HandleL2FrameAsync(l1Frame).ConfigureAwait(false);
        }
    }
}
