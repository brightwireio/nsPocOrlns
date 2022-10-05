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
        Trips = new List<TripRecord>();
    }
    public UnitTripStateEnum TripState { get; set; }
    public Guid? CurrentTripId { get; set; }
    public DateTime? CurrentTripStartDateTime { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public List<TripRecord> Trips { get; set; }
}

public record TripRecord(Guid Id, DateTime StartDateTime, DateTime EndDateTime);

public enum UnitTripStateEnum
{
    Inactive,
    Active
}