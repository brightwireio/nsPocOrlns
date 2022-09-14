namespace nsPocOrlns.TrafficSimulator;

public class SimulatorHttpV2
{

    private readonly IAssetMonRepository _repository;
    private readonly IEventHub _eventHub;

    public SimulatorHttpV2(IAssetMonRepository repository, IEventHub eventHub)
    {
        _repository = repository;
        _eventHub = eventHub;
    }

    [FunctionName("SimulatorHttpV2")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,ILogger log,[DurableClient] IDurableEntityClient client)
    {

        int batchSize = 500;
        int batchCounter = 0;
        long counter = 0;

        var events = _repository.GetUnitEvents();
        long numEvents = events.Count();
        var eventBatch = new List<object>();

        foreach (var @event in events)
        {
            counter++;
            batchCounter++;
            if (batchCounter >= batchSize || counter >= numEvents)
            {
                await _eventHub.SendBatch(eventBatch);
                eventBatch.Clear();
                batchCounter = 0;
            }
        }
        return new OkObjectResult("V0.11 :)");
    }
}
