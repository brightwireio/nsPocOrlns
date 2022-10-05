using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using nsPocOrlns.Common;
using nsPocOrlns.Grains.Interfaces;
using nsPocOrlns.Infrastructure.Persistence;
using nsPocOrlns.Infrastructure.Repositories;
using Orleans;
using Orleans.Runtime;

namespace nsPocOrlns.Grains;


public class TripGrain : Grain, ITripGrain
{
    private readonly ILogger _logger;
    private IAssetMonRepository _assetMonRepository;
    private IBlobService _blobService;
    private IPersistentState<TripState> _tripState;

    public TripGrain(ILogger<TripGrain> logger, [PersistentState("tripState")]  IPersistentState<TripState> tripState, IAssetMonRepository assetMonRepository, IBlobService blobService)
    {
        _logger = logger;
        _tripState = tripState;
        _assetMonRepository = assetMonRepository;
        _blobService = blobService;
    }

    public TripState GetState()
    {
        return _tripState.State;
    }

    public async Task ProcessEvent(UnitEvent @event)
    {
        await _tripState.State.ProcessEvent(@event);
        if ( _tripState.State.State == TripStateEnum.Complete )
        {
            if (_tripState.State.SavedToDb)
            {
                // update existing trip
                var tripEntity = _assetMonRepository.GetTrip(GetTripId());
                tripEntity.EndDateTime = _tripState.State.EndDateTime.Value;
                tripEntity.StartDateTime = _tripState.State.StartDateTime.Value;
                tripEntity.StartLatitude = _tripState.State.StartLatitude.Value;
                tripEntity.StartLongitude = _tripState.State.StartLongitude.Value;
                tripEntity.EndLatitude = _tripState.State.EndLatitude.Value;
                tripEntity.EndLongitude = _tripState.State.EndLongitude.Value;
                tripEntity.NumHarshBreaks = _tripState.State.NumHarshBreaks;
                tripEntity.NumViolations = _tripState.State.NumViolations;
            }
            else
            {
                // create new trip
                _assetMonRepository.AddTrip(new Infrastructure.Entities.Trip()
                {
                    UnitId = @event.Imei,
                    TripId = GetTripId(),
                    EndDateTime = _tripState.State.EndDateTime.Value,
                    StartDateTime = _tripState.State.StartDateTime.Value,
                    StartLatitude = _tripState.State.StartLatitude.Value,
                    StartLongitude = _tripState.State.StartLongitude.Value,
                    EndLatitude = _tripState.State.EndLatitude.Value,
                    EndLongitude = _tripState.State.EndLongitude.Value,
                    NumHarshBreaks = _tripState.State.NumHarshBreaks,
                    NumViolations = _tripState.State.NumViolations
                });
            }

            var eventData = JsonConvert.SerializeObject(_tripState.State.Events);
            _blobService.UploadBlob($"{GetTripId()}.json", eventData, "tripdata");
            _assetMonRepository.Save();

        }
    }

    private Guid GetTripId()
    {
        return this.GrainReference.GrainIdentity.PrimaryKey;
    }
    
}
