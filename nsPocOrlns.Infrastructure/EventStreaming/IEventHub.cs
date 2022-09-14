namespace nsPocOrlns.Infrastructure.EventStreaming;

public interface IEventHub
{
    public Task SendBatch(IEnumerable<object> events);

}
