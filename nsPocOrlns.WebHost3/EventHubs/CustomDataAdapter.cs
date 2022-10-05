using System.Text.Json;

namespace nsPocOrlns.WebHost3.EventHubs;

public class CustomDataAdapter : EventHubDataAdapter
{
    public CustomDataAdapter(SerializationManager serializationManager) : base(serializationManager)
    {
    }

    public override string GetPartitionKey(Guid streamGuid, string streamNamespace)
        => streamGuid.ToString();

    public override IStreamIdentity GetStreamIdentity(EventData queueMessage)
    {
        //DL - imei is long hidden

        //var guid = Guid.Parse(queueMessage.PartitionKey);

        var guid = GetGuidFromIme(queueMessage.PartitionKey);
        var ns = (string)queueMessage.Properties["StreamNamespace"];

        return new StreamIdentity(guid, ns);
    }

    private Guid GetGuidFromIme(string imei)
    {
        string s = $"00000000-0000-0000-0{imei.Substring(0,3)}-{imei.Substring(3, 12)}";
        return Guid.Parse(s);   
    }

    public override EventData ToQueueMessage<T>(Guid streamGuid, string streamNamespace, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        => throw new NotSupportedException("This adapter only supports read");

    protected override IBatchContainer GetBatchContainer(EventHubMessage eventHubMessage)
        => new CustomBatchContainer(eventHubMessage);
}

public class CustomBatchContainer : IBatchContainer
{
    public Guid StreamGuid { get; }

    public string StreamNamespace { get; }

    public StreamSequenceToken SequenceToken { get; }

    private readonly byte[] _payload;

    public CustomBatchContainer(EventHubMessage eventHubMessage)
    {
        StreamGuid = eventHubMessage.StreamIdentity.Guid;
        StreamNamespace = eventHubMessage.StreamIdentity.Namespace;
        SequenceToken = new EventHubSequenceTokenV2(eventHubMessage.Offset, eventHubMessage.SequenceNumber, 0);
        _payload = eventHubMessage.Payload;
    }

    public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
    {
        try
        {
            var evt = JsonSerializer.Deserialize<T>(_payload)!;
            return new[] { Tuple.Create(evt, SequenceToken) };
        }
        catch (Exception)
        {
            return new List<Tuple<T, StreamSequenceToken>>();
        }
    }

    public bool ImportRequestContext() => false;

    public bool ShouldDeliver(IStreamIdentity stream, object filterData, StreamFilterPredicate shouldReceiveFunc) => true;
}
