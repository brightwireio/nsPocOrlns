using Microsoft.AspNetCore.Mvc;
using nsPocOrlns.Grains.Interfaces;

namespace nsPocOrlns.WebHost.Controllers;

[ApiController]
[Route("units")]
public class UnitsController : ControllerBase
{
    private readonly ILogger<UnitsController> _logger;
    private readonly IClusterClient clusterClient;

    public UnitsController(ILogger<UnitsController> logger, IClusterClient clusterClient)
    {
        this._logger = logger;
        this.clusterClient = clusterClient;
    }


    [HttpGet("id")]
    public async Task<ActionResult<UnitDto>> Get(int id)
    {

        _logger.LogInformation("DL: received GET for unit {id}");

        var unitGrain = this.clusterClient.GetGrain<IUnitGrain>(id);
        var unitState = await unitGrain.GetState();
        var result = new UnitDto(id, unitState.Latitude, unitState.Longitude,"");
        return new ActionResult<UnitDto>(result);
    }
}


public record UnitDto(int Id, double Latitude, double Longitude, string Status);

