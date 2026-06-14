using PumaMQ.Server.Framings;
using PumaMQ.Server.Parsers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;

namespace PumaMQ.Server.Services;

internal class SocketFrameHandler
{
    private int _disposed = 0;

    private const int _boundedChannelCapacity = 128;

    private readonly TcpClient _tcpClient;

    private readonly Channel<RentedMemory> _channel;
    private readonly ChannelReader<RentedMemory> _channelReader;
    private readonly ChannelWriter<RentedMemory> _channelWriter;

    private readonly PipeReader _pipeReader;
    private readonly PipeWriter _pipeWriter;

    private readonly Task? _writeLoopTask;


    private SocketFrameHandler(TcpClient tcpClient)
    {
        //Todo: should throw ArgumentNullException?
        _tcpClient = tcpClient;
        NetworkStream stream = tcpClient.GetStream();
        _pipeReader = PipeReader.Create(stream);
        _pipeWriter = PipeWriter.Create(stream);

        BoundedChannelOptions options = new(_boundedChannelCapacity)
        {
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = System.Threading.Channels.Channel.CreateBounded<RentedMemory>(options);
        _channelReader = _channel.Reader;
        _channelWriter = _channel.Writer;

        CancellationTokenSource writeLoopCts = new CancellationTokenSource();
        _writeLoopTask = Task.Run(() => WriteLoopAsync(writeLoopCts.Token));
    }


    public static SocketFrameHandler Create(TcpClient tcpClient)
    {
        SocketFrameHandler socketFrameHandler = new(tcpClient);
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


    internal async Task WriteLoopAsync(CancellationToken cancellationToken = default)
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
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
        {
            return;
        }

        try
        {
            _channelWriter.Complete();
            _pipeWriter.Complete();
            _pipeReader.Complete();
            _tcpClient.Dispose();
        }
        catch
        {
        }
    }

}