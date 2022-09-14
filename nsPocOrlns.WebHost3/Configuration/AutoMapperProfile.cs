namespace nsPocOrlns.WebHost3.Configuration;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Trip, TripDto>();
        CreateMap<Unit, UnitDto>();
    }
}

