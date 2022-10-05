namespace nsPocOrlns.Infrastructure.EventStreaming;

public interface IEventHub
{
    public Task SendBatch(IEnumerable<object> events);

    public Task SendBatch(IEnumerable<object> events, string partitionKey);

}
