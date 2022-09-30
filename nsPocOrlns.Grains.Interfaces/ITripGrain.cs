using nsPocOrlns.Common;
using Orleans;

namespace nsPocOrlns.Grains.Interfaces;

public interface ITripGrain : IGrainWithGuidKey
{
    public Task ProcessEvent(ITripEvent @event);
}
