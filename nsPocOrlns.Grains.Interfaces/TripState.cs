using nsPocOrlns.Common;
using System.Reflection;

namespace nsPocOrlns.Grains.Interfaces;

public class TripState
{
    #region Private State
    private long _unitId;
    private Guid _tripId;
    private DateTime? _startDateTime;
    private DateTime? _endDateTime;
    private int _numHarshBreaks;
    private int _numViolations;
    private int _numEvents;
    private double? _startLatitude;
    private double? _startLongitude;
    private double? _currentLatitude;
    private double? _currentLongitude;
    private double? _endLatitude;
    private double? _endLongitude;
    private TripStateEnum _state;
    private List<UnitEvent> _events;
    private bool _savedToDb = false;
    #endregion

    #region Public Properties
    public long UnitId { get => _unitId; }
    public Guid TripId { get => _tripId; }
    public DateTime? StartDateTime { get => _startDateTime; }
    public DateTime? EndDateTime { get => _endDateTime; }
    public int NumHarshBreaks { get => _numHarshBreaks; }
    public int NumViolations { get => _numViolations; }
    public int NumEvents { get => _numEvents; }
    public double? StartLatitude { get => _startLatitude; }
    public double? StartLongitude { get => _startLongitude; }
    public double? EndLatitude { get => _endLatitude; }
    public double? EndLongitude { get => _endLongitude; }
    public TripStateEnum State { get => _state; }
    public double? CurrentLatitude { get => _currentLatitude; }
    public double? CurrentLongitude { get => _currentLongitude; }
    public List<UnitEvent> Events { get => _events; set => _events = value; }
    public bool SavedToDb { get => _savedToDb; }
    #endregion

    #region Constructors
    public TripState(long unitId, Guid tripId)
    {
        _unitId = unitId;
        _tripId = tripId;
        _events = new List<UnitEvent>();
    }

    public TripState()
    {
        _events = new List<UnitEvent>();
    }
        
    #endregion

    #region Public Methods
    public async Task ProcessEvent(UnitEvent @event)
    {
        if (IsDuplicate(@event))
        {
            return;
        }
        else if (IsOutOfOrder(@event))
        {
            await ProcessOutOfOrderEvent(@event);
        }
        else
        {
            _events.Add(@event);
            await GetProcessor(@event).Invoke(@event);
            _numEvents++;
        }
    }
    #endregion

    #region Private Methods

    private async Task ProcessOutOfOrderEvent(UnitEvent @event)
    {
        _events.Add(@event);
        var orderedEvents = new List<UnitEvent>(_events.OrderBy(v => v.LocationDateTime));
        Reset();
        foreach(var orderedEvent in orderedEvents)
        {
            await ProcessEvent(orderedEvent);
        }
    }

    private void Reset()
    {
        _events.Clear();
        _startDateTime = null;
        _endDateTime = null;
        _numHarshBreaks = 0;
        _numViolations = 0;
        _numEvents = 0;
        _startLatitude = null;
        _startLongitude = null;
        _currentLatitude = null;
        _currentLongitude = null;
        _endLatitude = null; 
        _endLongitude = null;
        _state = TripStateEnum.NotStarted;
    }

private bool IsDuplicate(UnitEvent @event)
    {
        return _events.Where(v => v.Id == @event.Id).Any();
    }
    private bool IsOutOfOrder(UnitEvent @event)
    {
        return @event.LocationDateTime < _events.MaxBy(v=>v.LocationDateTime)?.LocationDateTime;
    }

    private Func<UnitEvent, Task> GetProcessor(UnitEvent @event)
    {
        switch (@event.EventId)
        {
            case EventTypeEnum.IgnitionOn:
                return ProcessIgnitionOn;
            case EventTypeEnum.IgnitionOff:
                return ProcessIgnitionOff;
            case EventTypeEnum.Location:
                return ProcessLocation;
            default:
                return UnsupportedEventType;
        }
    }
    private async Task ProcessIgnitionOn(UnitEvent @event)
    {
        _startDateTime = @event.LocationDateTime;
        _startLatitude = @event.Latitude;
        _startLongitude = @event.Longitude;
        _state = TripStateEnum.InProgress;
    }
    private async Task ProcessIgnitionOff(UnitEvent @event)
    {
        _endDateTime = @event.LocationDateTime;
        _endLatitude = @event.Latitude;
        _endLongitude = @event.Longitude;
        _state = TripStateEnum.Complete;
    }
    private async Task ProcessLocation(UnitEvent @event)
    {
        _currentLatitude = @event.Latitude;
        _currentLongitude = @event.Longitude;
    }
    private async Task ProcessHarshBreak(UnitEvent @event)
    {
        _numHarshBreaks++;
    }
    private async Task ProcessViolation(UnitEvent @event)
    {
        _numViolations++;
    }

    private async Task UnsupportedEventType(UnitEvent @event)
    {

    }

    #endregion

}


public enum TripStateEnum
{
    NotStarted,
    InProgress,
    Complete
}