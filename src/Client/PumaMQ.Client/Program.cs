using PumaMQ.Client.Consumers;
using PumaMQ.Client.Services;


internal partial class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Starting client");

        Connection connection = await Connection.CreateAsync();

        Channel channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("Secondary-Ex");

        Consumer consumer = new Consumer();

        consumer.BasicConsumed += Consumer_BasicConsumed;

        await channel.BasicConsumeAsync("Main-q", "Main-con", consumer);

        //ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes("This is 1st message");

        //await connection.BasicPublishAsycn("main-ex", "main-rk", true, body);

        Console.ReadLine();
    }


    private static Task Consumer_BasicConsumed(object sender, BasicConsumeAsyncEventArgs args)
    {
        string message = $"Consumer is registered on Broker with Tag: {args.ConsumerTag}";
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}