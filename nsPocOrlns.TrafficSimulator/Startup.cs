using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Startup))]
namespace nsPocOrlns.TrafficSimulator;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var dbConnString = Environment.GetEnvironmentVariable("DbConnectionString");

        builder.Services.AddDbContext<AssetMonDbContext>(o => o.UseSqlServer(dbConnString));
        builder.Services.AddScoped<IAssetMonRepository, AssetMonRepository>();
        builder.Services.AddOptions<EventHubOptions>()
             .Configure<IConfiguration>((settings, configuration) =>
             {
                 configuration.GetSection("EventHub").Bind(settings);
             });
        builder.Services.AddScoped<IEventHub, AzureEventHub>();
    }
}
