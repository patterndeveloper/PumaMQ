using System.Net.Sockets;

namespace PumaMQ.Client.Services;

internal class SocketWrapper : IDisposable
{
    public Socket Socket { get; }
    public NetworkStream Stream { get; }


    public SocketWrapper(Socket socket, NetworkStream networkStream)
    {
        Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        Stream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
    }

    public void Dispose()
    {
        Socket.Dispose();
        Stream.Dispose();
    }
}