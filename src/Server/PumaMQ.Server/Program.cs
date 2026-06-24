using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PumaMQ.Server.Persistences;
using PumaMQ.Server.Services;


internal partial class Program
{
    private static async Task Main(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

        hostBuilder.ConfigureAppConfiguration(cfg =>
        {
            cfg.AddJsonFile("appsettings.json", optional: false);
        });

        hostBuilder.ConfigureServices((ctx, srv) =>
        {
            srv.AddOptions<EndPointOption>().BindConfiguration("EndPointOption");
            srv.AddOptions<DbOption>().BindConfiguration("DbOption");

            srv.AddSingleton<DbConnectionFactory>();
            srv.AddSingleton<ConnectionRepository>();
            srv.AddSingleton<ChannelRepository>();
            srv.AddSingleton<ConsumerRepository>();
            srv.AddSingleton<ExchangeRepository>();
            srv.AddSingleton<QueueRepository>();

            //srv.AddSingleton<SocketFrameHandler>();
            //srv.AddTransient<L2Parser>();
            //srv.AddTransient<SocketFrameHandler>();

            srv.AddSingleton<BrokerServer>();
        });

        IHost host = hostBuilder.Build();

        BrokerServer brokerServer = host.Services.GetRequiredService<BrokerServer>();

        await brokerServer.Connect();

        Console.ReadLine();
    }
}