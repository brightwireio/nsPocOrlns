using nsPocOrlns.Common;
using Orleans;

namespace nsPocOrlns.Grains.Interfaces;

public interface ITripGrain : IGrainWithGuidKey
{
    public Task ProcessEvent(UnitEvent @event);

    public TripState GetState();
}
