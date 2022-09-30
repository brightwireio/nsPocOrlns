﻿using Microsoft.Extensions.Logging;
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

    }

    private async Task ProcessIgnitionOff(UnitEvent @event)
    {

    }

    private async Task ProcessLocation(UnitEvent @event)
    {

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


