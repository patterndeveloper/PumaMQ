using PumaMQ.Server.Exceptions;
using System.Net;
using System.Net.Sockets;

namespace PumaMQ.Server.Services;

internal class TcpListenerFactory
{
    internal static TcpListener CreateAndStart(IPEndPoint iPEndPoint)
    {
        TcpListener tcpListener = new TcpListener(iPEndPoint);

        Start(tcpListener, iPEndPoint);

        return tcpListener;
    }

    internal static void Start(TcpListener tcpListener, IPEndPoint iPEndPoint)
    {
        try
        {
            tcpListener.Start();
        }
        catch (Exception ex)
        {
            tcpListener.Dispose();

            string message = $"Error in starting broker endpoint at address: {iPEndPoint.Address}, port: {iPEndPoint.Port}";
            ConnectionFailureException exception = new(message, ex);
            throw exception;
        }
    }
}
