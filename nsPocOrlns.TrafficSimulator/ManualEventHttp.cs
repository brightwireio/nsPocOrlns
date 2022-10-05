using Azure.Messaging.EventHubs.Producer;
using System.IO;

namespace nsPocOrlns.TrafficSimulator;

public class ManualEventHttp
{

    private readonly IAssetMonRepository _repository;
    private readonly IEventHub _eventHub;

    public ManualEventHttp(IAssetMonRepository repository, IEventHub eventHub)
    {
        _repository = repository;
        _eventHub = eventHub;
    }

    [FunctionName("ManualEventHttp")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,ILogger log,[DurableClient] IDurableEntityClient client)
    {

        var reader = new StreamReader(req.Body);
        var bodyContent = reader.ReadToEnd();
        var @event = JsonConvert.DeserializeObject<UnitEvent>(bodyContent);



        var eventBatch = new List<object>();
        eventBatch.Add(@event);
        await _eventHub.SendBatch(eventBatch, @event.Imei.ToString());

        return new OkObjectResult("done :)");
    }
}
