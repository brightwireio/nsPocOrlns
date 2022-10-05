using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using nsPocOrlns.Common;
using nsPocOrlns.Grains.Interfaces;
using nsPocOrlns.Infrastructure.Persistence;
using nsPocOrlns.Infrastructure.Repositories;
using Orleans;
using Orleans.Runtime;
using System.Threading.Tasks;

namespace nsPocOrlns.Grains;

public class UnitGrain : Grain, IUnitGrain
{

    private IPersistentState<UnitState> _unitState;
    private ILogger _logger;
    private IAssetMonRepository _assetMonRepository;
    private IBlobService _blobService;

    public UnitGrain(ILogger<UnitGrain> logger, [PersistentState("unitState")] IPersistentState<UnitState> unitState, IAssetMonRepository assetMonRepository, IBlobService blobService)
    {
        _logger = logger;
        _unitState = unitState;
        _assetMonRepository = assetMonRepository;
        _blobService = blobService;
    }


    // THIS IS ONE APPROACH TO PERIODICALLY PERSIST STATE, RATHER THAN ON REACH EVENT - FOR PERFORMANCE
    // MIGHT NOT BE VIABLE THOUGH DUE TO POTENTIAL DATA LOSS.
    //public override Task OnActivateAsync()
    //{
    //    this.RegisterTimer(this.TimerCallback, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
    //    return base.OnActivateAsync();
    //}

    //Task TimerCallback(object callbackstate)
    //{
    //    if ( _isDirty )
    //    {
    //        _unitState.WriteStateAsync();
    //        _isDirty = false;
    //    }
    //    return Task.CompletedTask;
    //}

    public async Task<UnitState> GetState()
    {
        return _unitState.State;
    }


    public async Task ProcessEvent(UnitEvent @event)
    {
        Func<UnitEvent, Task> processor = null;

        _logger.LogInformation($"DL: Processing event {@event.EventId} for unit {this.IdentityString}");
        switch (@event.EventId)
        {
            case EventTypeEnum.IgnitionOn:
                processor = ProcessIgnitionOn;
                break;
            case EventTypeEnum.IgnitionOff:
                processor = ProcessIgnitionOff;
                break;
            case EventTypeEnum.Location:
                processor = ProcessLocation;
                break;
        }

        if (processor != null)
        {
            await processor(@event);
        }

    }

    private async Task ProcessIgnitionOn(UnitEvent @event)
    {
        if ( _unitState.State.TripState == UnitTripStateEnum.Inactive )
        {
            _unitState.State.TripState = UnitTripStateEnum.Active;
            _unitState.State.CurrentTripId = Guid.NewGuid();
            _unitState.State.CurrentTripStartDateTime = @event.LocationDateTime;
            var tripGrain = this.GrainFactory.GetGrain<ITripGrain>(_unitState.State.CurrentTripId.Value);
            await tripGrain.ProcessEvent(@event);
            await _unitState.WriteStateAsync();
        }
    }

    private async Task ProcessIgnitionOff(UnitEvent @event)
    {
        if (_unitState.State.TripState == UnitTripStateEnum.Inactive)
        {
            //no trip active - event must be previous trip? What if location comes before ignition on?
            // would need to stash and wait for matching ignition on?
        }

        if (_unitState.State.TripState == UnitTripStateEnum.Active && @event.LocationDateTime > _unitState.State.CurrentTripStartDateTime)
        {
            //event is for current active trip
            var tripGrain = this.GrainFactory.GetGrain<ITripGrain>(_unitState.State.CurrentTripId.Value);
            await tripGrain.ProcessEvent(@event);
            var tripState = tripGrain.GetState();
            _unitState.State.Trips.Add(new TripRecord(_unitState.State.CurrentTripId.Value, tripState.StartDateTime.Value, tripState.EndDateTime.Value));
            _unitState.State.CurrentTripStartDateTime = null;
            _unitState.State.CurrentTripId = null;
            _unitState.State.TripState = UnitTripStateEnum.Inactive;
        }

        if (_unitState.State.TripState == UnitTripStateEnum.Active && @event.LocationDateTime < _unitState.State.CurrentTripStartDateTime)
        {
            //event is for previous trip
            var trip = _assetMonRepository.GetTripForEvent(@event.Imei, @event.LocationDateTime);
            if (trip != null)
            {
                var tripGrain = this.GrainFactory.GetGrain<ITripGrain>(trip.TripId);
                await tripGrain.ProcessEvent(@event);
            }
        }

        
        await _unitState.WriteStateAsync();

    }

    private async Task ProcessLocation(UnitEvent @event)
    {
        if (_unitState.State.TripState == UnitTripStateEnum.Inactive)
        {
            //no trip active - event must be previous trip? What if location comes before ignition on?
            // would need to stash and wait for matching ignition on?
        }

        if ( _unitState.State.TripState == UnitTripStateEnum.Active && @event.LocationDateTime > _unitState.State.CurrentTripStartDateTime )
        {
            //event is for current active trip
            var tripGrain = this.GrainFactory.GetGrain<ITripGrain>(_unitState.State.CurrentTripId.Value);
            await tripGrain.ProcessEvent(@event);
        }

        if (_unitState.State.TripState == UnitTripStateEnum.Active && @event.LocationDateTime < _unitState.State.CurrentTripStartDateTime)
        {
            //event is for previous trip
            var trip = _assetMonRepository.GetTripForEvent(@event.Imei,@event.LocationDateTime);
            if (trip != null)
            {
                var tripGrain = this.GrainFactory.GetGrain<ITripGrain>(trip.TripId);
                await tripGrain.ProcessEvent(@event);
            }
        }
    }



    //DL: olv version 
    //public async Task ProcessEvent(UnitEvent @event)
    //{
    //    _logger.LogInformation($"DL: Processing event {@event.EventId} for unit {this.IdentityString}");
    //    _isDirty = true;
    //    _unitState.State.Latitude = @event.Latitude;
    //    _unitState.State.Longitude = @event.Longitude;
    //    _unitState.State.Events.Add(@event);

    //    switch (@event.EventId)
    //    {
    //        case EventTypeEnum.IgnitionOn:
    //            _unitState.State.TripState = TripStateEnum.Active;
    //            _unitState.State.TripStartDateTime = DateTime.Now;
    //            break;
    //        case EventTypeEnum.IgnitionOff:
    //            _unitState.State.TripState = TripStateEnum.Inactive;
    //            Guid tripId = Guid.NewGuid();
    //            _assetMonRepository.AddTrip(new Infrastructure.Entities.Trip()
    //            {
    //                UnitId = @event.Imei,
    //                StartDateTime = _unitState.State.TripStartDateTime.Value,
    //                EndDateTime = DateTime.Now,
    //                TripId = tripId,
    //                NumHarschBreaks = _unitState.State.NumHarschBreaks,
    //                NumViolations = _unitState.State.NumViolations
    //            });

    //            var eventData = JsonConvert.SerializeObject(_unitState.State.Events);
    //            _blobService.UploadBlob($"{tripId}.json", eventData, "tripdata");
    //            _assetMonRepository.Save();
    //            _unitState.State.Events.Clear();
    //            _unitState.State.NumHarschBreaks = 0;
    //            _unitState.State.NumViolations = 0;
    //            break;
    //        case EventTypeEnum.Location:
    //            break;
    //        case EventTypeEnum.SpeedLimitViolation:
    //            _unitState.State.NumViolations++;
    //            break;
    //        case EventTypeEnum.HarshBreak:
    //            _unitState.State.NumHarschBreaks++;
    //            break;

    //    }

    //    try
    //    {
    //        // IF USING TIMED BASED PERSISTENCE OF STATE (COMMENTED OUT ABOVE), THEN COMMENT THIS OUT
    //        await _unitState.WriteStateAsync();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "DL: Error persisting grain!!");
    //    }

    //}
}


