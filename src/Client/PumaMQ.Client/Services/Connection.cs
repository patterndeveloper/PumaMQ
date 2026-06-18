using PumaMQ.Client.Framings;
using System.Buffers;

namespace PumaMQ.Client.Services;

public class Connection
{
    private readonly ChannelNoManager _channelNoManager;
    private readonly Dictionary<int, Channel> _channels = [];
    private readonly SocketFrameHandler _socketFrameHandler;

    private readonly Timer _heartbeatWriteTimer;
    private readonly Timer _heartbeatReadTimer;

    private readonly TimeSpan _writeHeartBeatDueTime = TimeSpan.FromSeconds(100);
    private readonly TimeSpan _readHeartBeatDueTime = TimeSpan.FromSeconds(100);

    private readonly CancellationTokenSource _mainLoopCts = new CancellationTokenSource();

    private volatile bool _closed;


    private Connection(SocketFrameHandler socketFrameHandler)
    {
        _channelNoManager = new();

        _socketFrameHandler = socketFrameHandler ?? throw new ArgumentNullException(nameof(socketFrameHandler));

        _heartbeatWriteTimer = new Timer(callback: OnWriteHeartBeatTimer,
                                         state: null,
                                         dueTime: _writeHeartBeatDueTime / 10,
                                         period: Timeout.InfiniteTimeSpan);

        _heartbeatReadTimer = new Timer(callback: OnReadHeartBeatTimer,
                                        state: null,
                                        dueTime: _readHeartBeatDueTime / 10,
                                        period: Timeout.InfiniteTimeSpan);

        CancellationTokenSource readLoopCts = new CancellationTokenSource();
        Task readLoopTask = Task.Run(() => ReadLoopAsync(readLoopCts.Token));
    }


    public static async Task<Connection> CreateAsync()
    {
        SocketFrameHandler socketFrameHandler = await SocketFrameHandler.CreateAsync();
        Connection connection = new(socketFrameHandler);
        return connection;
    }


    private async void OnWriteHeartBeatTimer(object? state)
    {
        if (_heartbeatWriteTimer == null)
        {
            //It is possible that a timer just after getting disposed, get fired. this condition prevent that.
            return;
        }

        /// A heartbeat frame has the following layout:
        /// +--------------------+------------------+-----------------+--------------------------+
        /// | Frame Type(1 byte) | Channel(2 bytes) | Length(4 bytes) | End Frame Marker(1 byte) |
        /// +--------------------+------------------+-----------------+--------------------------+
        /// | 0x08               | 0x0000           | 0x00000000      | 0xCE                     |
        /// +--------------------+------------------+-----------------+--------------------------+
        /// 

        byte[] heartBeatFrame = new byte[] { 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCE };

        byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(8);

        heartBeatFrame.CopyTo(rentedBuffer, 0);
        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(rentedBuffer, 0, heartBeatFrame.Length);
        RentedMemory rentedMemory = new RentedMemory(rentedBuffer, memory);

        try
        {
            if (!_closed)
            {
                await _socketFrameHandler.WriteAsync(rentedMemory, _mainLoopCts.Token).ConfigureAwait(false);
                _heartbeatWriteTimer.Change(_writeHeartBeatDueTime, Timeout.InfiniteTimeSpan);
            }
        }
        catch (Exception ex)
        {
            //Because this method is void, the exception does not catch by caller and process crashes. so we swallow it.
            //Actual handling of this exception is done on broker, that send connection close frame
        }
    }


    private void OnReadHeartBeatTimer(object? state)
    {

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

            await channel.HandleReceivedFrameAsync(l1Frame);

            //if (!channelExist)
            //{
            //    channel = new Channel(_socketFrameHandler, l1Frame.Channel);
            //    _channels.Add(channel.ChannelNo, channel);
            //}

            //await channel!.HandleL2FrameAsync(l1Frame).ConfigureAwait(false);
        }
    }


    public async Task<Channel> CreateChannelAsync()
    {
        ushort channelNo = _channelNoManager.GetNext();

        Channel channel = Channel.Create(_socketFrameHandler, channelNo);
        _channels.TryAdd(channelNo, channel);
        await channel.ChannelOpenAsync();

        return channel;
    }
}