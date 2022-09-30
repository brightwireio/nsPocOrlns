namespace nsPocOrlns.TrafficSimulator;

[JsonObject(MemberSerialization.OptIn)]
public class UnitEntity
{

    int interval = 10;
    int senderUnitCount = 100; //number of units each sender handles
    ILogger _logger;

    public UnitEntity(ILogger logger)
    {
        _logger = logger;
    }


    [JsonProperty("tripLength")]
    public int TripLength { get; set; }

    [JsonProperty("currentLength")]
    public int CurrentLength { get; set; }

    [JsonProperty("unitState")]
    public UnitStateEnum UnitState { get; set; } = UnitStateEnum.Stationary;

    public void Start(int tripLength)
    {
        UnitState = UnitStateEnum.TripActive;
        TripLength = tripLength;
        CurrentLength = 0;
        _logger.LogInformation($"Trip of length {tripLength} starting for unit {Entity.Current.EntityId}");

        int unitNum = int.Parse(Entity.Current.EntityId.EntityKey);
        Random r = new Random();

        var @event = new UnitEvent()
        {
            Imei = unitNum,
            EventId = EventTypeEnum.IgnitionOn,
            ServerDateTime = DateTime.Now,
            LocationDateTime = DateTime.Now,
            Latitude = r.Next(0, 30),
            Longitude = r.Next(0, 30)
        };

        SendEvent(@event);

        Entity.Current.SignalEntity(Entity.Current.EntityId, DateTime.UtcNow.AddSeconds(interval), "LocationChanged");
    }

    public async Task LocationChanged()
    {
        Random r = new Random();
        int unitNum = int.Parse(Entity.Current.EntityId.EntityKey);
        if (CurrentLength < TripLength)
        {
            CurrentLength += 1;
            var @event = new UnitEvent()
            {
                Imei = unitNum,
                EventId = EventTypeEnum.Location,
                ServerDateTime = DateTime.Now,
                LocationDateTime = DateTime.Now,
                Latitude = r.Next(0, 30),
                Longitude = r.Next(0, 30)
            };
            
            _logger.LogInformation($"Unit {unitNum} location changed {CurrentLength} times");
            SendEvent(@event);
            Entity.Current.SignalEntity(Entity.Current.EntityId, DateTime.UtcNow.AddSeconds(interval), "LocationChanged");
        }
        else
        {
            _logger.LogInformation($"Unit {unitNum} trip completed!");
            var @event = new UnitEvent()
            {
                Imei = unitNum,
                EventId = EventTypeEnum.IgnitionOff,
                ServerDateTime = DateTime.Now,
                LocationDateTime = DateTime.Now,
                Latitude = r.Next(0, 30),
                Longitude = r.Next(0, 30)
            };
            SendEvent(@event);
        }
        UnitState = UnitStateEnum.Stationary;
    }

    private void SendEvent(UnitEvent @event)
    {
        int unitNum = int.Parse(Entity.Current.EntityId.EntityKey);
        int senderNum = Math.Abs(unitNum / senderUnitCount);
        var senderId = new EntityId("EventHubSender", senderNum.ToString());
        Entity.Current.SignalEntity(senderId, "SubmitMessage", @event);
    }


    [FunctionName(nameof(UnitEntity))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger logger)
        => ctx.DispatchAsync<UnitEntity>(logger);
}

public enum UnitStateEnum
{
    Stationary,
    TripActive
}
