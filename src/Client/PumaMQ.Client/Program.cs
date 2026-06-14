using PumaMQ.Client.Services;


internal partial class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Starting client");

        Connection connection = await Connection.CreateAsync();

        Channel channel = await connection.CreateChannelAsync();

        //ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes("This is 1st message");

        //await connection.BasicPublishAsycn("main-ex", "main-rk", true, body);

        Console.ReadLine();
    }
}