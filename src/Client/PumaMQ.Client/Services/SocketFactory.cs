using PumaMQ.Client.Exceptions;
using System.Net;
using System.Net.Sockets;

namespace PumaMQ.Client.Services;

internal class SocketFactory
{
    public static async Task<SocketWrapper> CreateAndConnectAsync(IPEndPoint serverEndPoint)
    {
        Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

        await ConnectAsync(socket, serverEndPoint);

        NetworkStream networkStream = new(socket);
        SocketWrapper socketAdaptor = new(socket, networkStream);
        return socketAdaptor;
    }


    private static async Task ConnectAsync(Socket socket, IPEndPoint serverEndPoint)
    {
        try
        {
            await socket.ConnectAsync(serverEndPoint);
        }
        catch (Exception ex)
        {
            socket.Dispose();

            string message = $"Error in connection to broker endpoint at address: {serverEndPoint.Address}, port: {serverEndPoint.Port}";
            ConnectionFailureException connectionFailureException = new(message, ex);
            throw connectionFailureException;
        }
    }
}
