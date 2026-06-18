using PumaMQ.Server.Framings;
using System.Net.Sockets;

namespace PumaMQ.Server.Services;

internal class Connection
{
    internal Guid Id { get; set; }

    private readonly SocketFrameHandler _socketFrameHandler;

    private readonly Dictionary<ushort, Channel> _channels = [];


    public Connection(TcpClient tcpClient)
    {
        Id = Guid.NewGuid();

        _socketFrameHandler = SocketFrameHandler.Create(tcpClient);

        Channel channel0 = new Channel(_socketFrameHandler, 0);
        _channels.Add(0, channel0);

        CancellationTokenSource readLoopCts = new CancellationTokenSource();
        Task readLoopTask = Task.Run(() => ReadLoopAsync(readLoopCts.Token));
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
                channel = new Channel(_socketFrameHandler, l1Frame.Channel);
                _channels.Add(channel.ChannelNo, channel);

                //2- else throw exception and close connection
            }

            await channel!.HandleL2FrameAsync(l1Frame).ConfigureAwait(false);
        }
    }
}
