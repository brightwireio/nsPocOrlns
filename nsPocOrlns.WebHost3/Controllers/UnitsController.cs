namespace nsPocOrlns.WebHost.Controllers;

[ApiController]
[Route("units")]
public class UnitsController : ControllerBase
{
    private readonly ILogger<UnitsController> _logger;
    private readonly IAssetMonRepository _assetMonRepository;
    private readonly IMapper _mapper;

    public UnitsController(ILogger<UnitsController> logger, IAssetMonRepository assetMonRepository, IMapper mapper)
    {
        _logger = logger;
        _assetMonRepository = assetMonRepository;
        _mapper = mapper;
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<UnitDto>> GetUnit(long id)
    {
        return new ActionResult<UnitDto>(_mapper.Map<UnitDto>(await _assetMonRepository.GetUnit(id)));
    }


    [HttpGet("{unitId}/trips")]
    public async Task<ActionResult<List<TripDto>>> GetUnitTrips(long unitId)
    {
        return _mapper.Map<List<TripDto>>(_assetMonRepository.GetTripsForUnit(unitId));
    }


    [HttpGet("{unitId}/trips/{tripId}/events")]
    public async Task<ActionResult<List<UnitEvent>>> GetTripEvents(long unitId, Guid tripId)
    {
        return _assetMonRepository.GetEventsForTrip(tripId);
    }
}









