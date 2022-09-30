using nsPocOrlns.Grains.Interfaces;
using Orleans.EventSourcing;
using Orleans.Providers;

namespace nsPocOrlns.Grains;


public class TripGrain : JournaledGrain<TripState, ITripEvent>, ITripGrain
{
    public async Task ProcessEvent(ITripEvent @event)
    {

        
    }
}
