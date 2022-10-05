namespace nsPocOrlns.Infrastructure.Repositories;

public interface IAssetMonRepository : IRepository
{
    IEnumerable<Trip> GetTripsForUnit(long unitId);

    Trip GetTripForEvent(long unitId, DateTime eventDateTime);

    Trip GetTrip(Guid id);

    void AddTrip(Trip trip);

    void AddUnitEvent(UnitEvent @event);

    IEnumerable<UnitEvent> GetUnitEvents();

    List<UnitEvent> GetEventsForTrip(Guid tripId);

    Task<Unit> GetUnit(long unitId);
}
