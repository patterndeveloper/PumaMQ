using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PumaMQ.Server.Persistences;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PumaMQ.Server.Services;

internal class BrokerServer
{
    private readonly IPAddress _localIp;
    private int _localPort;

    internal Dictionary<int, Connection> _connections = [];
    private readonly IServiceProvider _serviceProvider;


    public BrokerServer(IServiceProvider serviceProvider, IOptions<EndPointOption> options)
    {
        _serviceProvider = serviceProvider;
        _localIp = IPAddress.Parse(options.Value.IpAddress);
        _localPort = options.Value.Port;
    }


    internal async Task Connect()
    {
        IPEndPoint localEndPoint = new(_localIp, _localPort);

        using TcpListener tcpListener = TcpListenerFactory.CreateAndStart(localEndPoint);

        Console.WriteLine($"Server started on port #{localEndPoint.Port}");

        while (true)
        {
            var tcpClient = await tcpListener.AcceptTcpClientAsync();

            Connection connection = await Connection.CreateAsync(_serviceProvider, tcpClient);
            _connections.Add(connection.Id, connection);
        }
    }


    internal async Task ProcessRequest(NetworkStream stream)
    {
        //Create 1 Connection and many Channels object (as requested)

        PipeReader pipeReader = PipeReader.Create(stream);
        PipeWriter pipeWriter = PipeWriter.Create(stream);

        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            do
            {
                Console.WriteLine("Processing new Connection ...");

                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine($"Received message is {message}");
                }
            }
            while (bytesRead > 0);

            Console.WriteLine("Press Enter to finish worker socket");
        }
        catch (Exception ex)
        {
            Exception exception = ex;
        }
        finally
        {
            stream.Dispose();
        }
    }
}