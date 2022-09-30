using nsPocOrlns.Common;
using Orleans;

namespace nsPocOrlns.Grains.Interfaces;

public interface IUnitGrain : IGrainWithIntegerKey
{
    public Task ProcessEvent(UnitEvent @event);

    public Task<UnitState> GetState();
}


public class UnitState
{
    public UnitState()
    {
    }
    public TripStateEnum TripState { get; set; }

    public Guid? CurrentTripId { get; set; }

    public DateTime? CurrentTripStartDateTime { get; set; }
}

public enum TripStateEnum
{
    Inactive,
    Active
}