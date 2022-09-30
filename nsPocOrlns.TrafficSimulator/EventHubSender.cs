namespace nsPocOrlns.TrafficSimulator;


[JsonObject(MemberSerialization.OptIn)]
public class EventHubSender
{

    private readonly int batchSize = 1;
    private readonly ILogger _logger;
    private readonly IAssetMonRepository _repository;
    private readonly IEventHub _eventHub;

    [JsonProperty("messageBlob")]
    public string MessageBlob { get; set; }

    public EventHubSender(ILogger logger, IAssetMonRepository repository, IEventHub eventHub)
    {
        _logger = logger;
        _repository = repository;
        _eventHub = eventHub;
    }

    //THIS VERSION SENDS TO EVENT HUB. USED TO TRY AND CREATE A MORE REALITIC STREAM OF EVENTS FROM UNITS AT CONFIGURABLE INTERVALS
    public async Task SubmitMessage(UnitEvent newMessage)
    {
        bool forceSend = false;
        if (newMessage.EventId == EventTypeEnum.IgnitionOff)
        {
            _logger.LogInformation($"Initiating force send due to IGNITION-OFF");
            forceSend = true;
        }
        var messages = new List<UnitEvent>();
        if (!string.IsNullOrEmpty(MessageBlob))
        {
            messages = JsonConvert.DeserializeObject<List<UnitEvent>>(MessageBlob);
        }
        messages.Add(newMessage);
        if (messages.Count >= batchSize || forceSend)
        {
            try
            {
                await _eventHub.SendBatch(messages);
                messages.Clear();
                MessageBlob = JsonConvert.SerializeObject(messages);
                _logger.LogInformation($"Sending batch of {batchSize} to event hub");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error trying to send to event hub {ex.Message}");
            }
        }
        MessageBlob = JsonConvert.SerializeObject(messages);
    }


    //THIS VERSION SENDS TO A SQL DB, WHICH IS THEN USED FROM SimulatorHttpV2 TO CREATE A SUPER FAST STREAM OF EVENTS TO TEST HIGH LOAD
    //public async Task SubmitMessage(UnitEvent newMessage)
    //{
    //    bool forceSend = false;
    //    if (newMessage.EventId == EventTypeEnum.IgnitionOff)
    //    {
    //        _logger.LogInformation($"Initiating force send due to IGNITION-OFF");
    //        forceSend = true;
    //    }

    //    var messages = new List<UnitEvent>();
    //    if (!string.IsNullOrEmpty(MessageBlob))
    //    {
    //        messages = JsonConvert.DeserializeObject<List<UnitEvent>>(MessageBlob);
    //    }
    //    messages.Add(newMessage);

    //    if (messages.Count >= batchSize || forceSend)
    //    {
    //        try
    //        {
    //            foreach (var message in messages)
    //            {
    //                _repository.AddUnitEvent(message);
    //            }
    //            _repository.Save();
    //            messages.Clear();
    //            MessageBlob = JsonConvert.SerializeObject(messages);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogWarning($"Error trying to send to event hub {ex.Message}");
    //        }
    //    }
    //    MessageBlob = JsonConvert.SerializeObject(messages);
    //}

    [FunctionName(nameof(EventHubSender))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger logger)
        => ctx.DispatchAsync<EventHubSender>(logger);
}
