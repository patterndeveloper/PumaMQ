using PumaMQ.Client.Framings;
using PumaMQ.Client.Parsers;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Channels;

namespace PumaMQ.Client.Services;

internal class SocketFrameHandler : IDisposable
{
    private int _disposed = 0;

    private static readonly IPAddress _serverIp = IPAddress.Loopback;
    private const int _serverPort = 8123;
    private const int _boundedChannelCapacity = 128;

    private readonly SocketWrapper _socketAdaptor;

    private readonly Channel<RentedMemory> _channel;
    private readonly ChannelReader<RentedMemory> _channelReader;
    private readonly ChannelWriter<RentedMemory> _channelWriter;

    private readonly PipeReader _pipeReader;
    private readonly PipeWriter _pipeWriter;

    private readonly Task? _writeLoopTask;


    private SocketFrameHandler(SocketWrapper socketAdaptor)
    {
        _socketAdaptor = socketAdaptor;
        _pipeReader = PipeReader.Create(_socketAdaptor.Stream);
        _pipeWriter = PipeWriter.Create(_socketAdaptor.Stream);

        BoundedChannelOptions channelOptions = new BoundedChannelOptions(_boundedChannelCapacity)
        {
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        };

        _channel = System.Threading.Channels.Channel.CreateBounded<RentedMemory>(channelOptions);
        _channelReader = _channel.Reader;
        _channelWriter = _channel.Writer;

        CancellationTokenSource writeLoopCts = new CancellationTokenSource();
        _writeLoopTask = Task.Run(() => WriteLoopAsync(writeLoopCts.Token));
    }


    internal static async Task<SocketFrameHandler> CreateAsync()
    {
        IPEndPoint serverEndPoint = new(_serverIp, _serverPort);
        SocketWrapper socketAdaptor = await SocketFactory.CreateAndConnectAsync(serverEndPoint);
        SocketFrameHandler socketFrameHandler = new(socketAdaptor);
        return socketFrameHandler;
    }


    internal async Task WriteAsync(RentedMemory rentedMemory, CancellationToken cancellationToken = default)
    {
        await _channelWriter.WriteAsync(rentedMemory, cancellationToken);
    }

    internal async Task<L1Frame> ReadL1FrameAsync(CancellationToken cancellationToken = default)
    {
        L1Frame l1Frame = await L1Parser.ReadFrameFromPipeAsync(_pipeReader, cancellationToken);
        return l1Frame;
    }


    internal async Task WriteLoopAsync(CancellationToken cancellationToken)
    {
        //Because this task is fire & forget, put all the logic in try catch block
        try
        {
            while (await _channelReader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                RentedMemory rentedMemory = await _channelReader.ReadAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    await _pipeWriter.WriteAsync(rentedMemory.Memory, cancellationToken).ConfigureAwait(false);
                }
                finally 
                { 
                    rentedMemory.Dispose();
                }

                await _pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            //Todo: log ex
            throw;
        }
    }


    public void Dispose()
    {
        if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
        {
            return;
        }

        try
        {
            _channelWriter.Complete();
            _pipeWriter.Complete();
            _pipeReader.Complete();
            _socketAdaptor.Dispose();
        }
        catch
        {
        }
    }
}