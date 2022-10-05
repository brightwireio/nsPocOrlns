
using Orleans.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var asConnString = builder.Configuration["blobStoreConnString"];
var ehConnString = builder.Configuration["eventHubConnString"];
var ehEventHubName = builder.Configuration["eventHubName"];


builder.Services.AddScoped<IBlobService>(o => new AzureBlobService(asConnString));
builder.Services.AddDbContext<AssetMonDbContext>(o =>
{
    o.UseSqlServer( builder.Configuration.GetConnectionString("assetDbConnection"), sqlo => sqlo.EnableRetryOnFailure());
});
builder.Services.AddScoped<IAssetMonRepository, AssetMonRepository>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));



//NOTE: USE THIS FOR RUNNING CLUSTER ON DEV/LOCAL MACHINE
builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
    .UseLocalhostClustering()
    .Configure<ClusterOptions>(opts =>
    {
        opts.ClusterId = "dev";
        opts.ServiceId = "oamAPI";
    })
    .Configure<EndpointOptions>(opts =>
    {
        opts.AdvertisedIPAddress = IPAddress.Loopback;
    })
    .AddAzureTableGrainStorage("PubSubStore", options => options.ConfigureTableServiceClient("DefaultEndpointsProtocol=https;AccountName=nspocorleansstorage;AccountKey=9+D4sixvMcR2Sfg7dEGRlZYnYcc7jDJFDY+L0RlxvUogsnTdsLJhwb3AJ1DqtqIKk8piwBCXXdbj+AStfTfngA==;EndpointSuffix=core.windows.net"))
    //.AddCosmosDBGrainStorageAsDefault(opt =>
    //{
    //    opt.AccountEndpoint = "https://cdbacct-nspoc-orleans.documents.azure.com:443/";
    //    opt.AccountKey = "AbkLMFQxTdJlsXnYixzRJNby19HFrOdH83h1d8Wb611u3ELYaC293aepFObCL0H9VdKWwG6o9seiAAWjNiJBPA==";
    //    opt.DB = "container1";
    //    opt.CanCreateResources = true;
    //})
    .AddAzureBlobGrainStorageAsDefault(opt =>
    {
        opt.ConfigureBlobServiceClient("DefaultEndpointsProtocol=https;AccountName=nspocorleansstorage;AccountKey=9+D4sixvMcR2Sfg7dEGRlZYnYcc7jDJFDY+L0RlxvUogsnTdsLJhwb3AJ1DqtqIKk8piwBCXXdbj+AStfTfngA==;EndpointSuffix=core.windows.net");
        //opt.ContainerName = "grains";
    })

    .AddEventHubStreams(
        "my-stream-provider",
        (ISiloEventHubStreamConfigurator configurator) =>
        {
            configurator.ConfigureEventHub(builder => builder.Configure(options =>
            {
                options.ConfigureEventHubConnection(
                    ehConnString,
                    ehEventHubName,
                    "$Default");

            }));

            configurator.UseDataAdapter(
                (sp, n) => ActivatorUtilities.CreateInstance<CustomDataAdapter>(sp));

            configurator.UseAzureTableCheckpointer(
                builder => builder.Configure(options =>
                {
                    options.ConfigureTableServiceClient("DefaultEndpointsProtocol=https;AccountName=nspocorleansstorage;AccountKey=9+D4sixvMcR2Sfg7dEGRlZYnYcc7jDJFDY+L0RlxvUogsnTdsLJhwb3AJ1DqtqIKk8piwBCXXdbj+AStfTfngA==;EndpointSuffix=core.windows.net");
                    options.PersistInterval = TimeSpan.FromSeconds(10);
                    
                }));
        });
});



//NOTE: USE THIS FOR RUNNING ON A FULL SERVER CLUSTER
//builder.Host.UseOrleans(siloBuilder =>
//{
//    int siloPort = 11111;
//    int gatewayPort = 22222;
//    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_PORTS")))
//    {
//        var strPorts = Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_PORTS").Split(',');
//        if (strPorts.Length >= 2)
//        {
//            siloPort = int.Parse(strPorts[0]);
//            gatewayPort = int.Parse(strPorts[1]);
//        }
//    }
//    siloBuilder
//    .ConfigureEndpoints(IPAddress.Parse(Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_IP")), siloPort: siloPort, gatewayPort: gatewayPort, listenOnAnyHostAddress: true)
//    .Configure<ClusterOptions>(opts =>
//    {
//        opts.ClusterId = "nsPOCOrlnsCluster";
//        opts.ServiceId = "nsPOCOrlnsAPI";
//    })

//    .AddAzureTableGrainStorage("PubSubStore", options => options.ConfigureTableServiceClient(asConnString))
//    .UseAzureStorageClustering(options=>
//    {
//        options.ConfigureTableServiceClient(asConnString);

//    })

//    .AddAzureBlobGrainStorageAsDefault(opt =>
//    {
//        opt.ConfigureBlobServiceClient(asConnString);
//    })
//    .AddEventHubStreams(
//        "my-stream-provider",
//        (ISiloEventHubStreamConfigurator configurator) =>
//        {
//            configurator.ConfigureEventHub(builder => builder.Configure(options =>
//            {
//                options.ConfigureEventHubConnection(
//                    ehConnString,
//                    ehEventHubName,
//                    "$Default");

//            }));

//            configurator.UseDataAdapter(
//                (sp, n) => ActivatorUtilities.CreateInstance<CustomDataAdapter>(sp));
//            configurator.UseAzureTableCheckpointer(
//                builder => builder.Configure(options =>
//                {
//                    options.ConfigureTableServiceClient(asConnString);
//                    options.PersistInterval = TimeSpan.FromSeconds(10);
//                }));
//        });
//});



var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
