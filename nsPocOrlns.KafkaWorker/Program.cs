using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;
using nsPocOrlns.Infrastructure.EventStreaming;
using nsPocOrlns.KafkaWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices( (context, services) =>
    {

        services.Configure<EventHubOptions>(context.Configuration.GetSection("EventHub"));
        services.AddOptions<EventHubOptions>();
        services.AddSingleton<IEventHub, AzureEventHub>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
