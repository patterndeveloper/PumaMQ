using PumaMQ.Server.Services;


internal partial class Program
{
    private static async Task Main(string[] args)
    {
        BrokerServer handler = new BrokerServer();

        await handler.Connect();

        Console.ReadLine();
    }
}

