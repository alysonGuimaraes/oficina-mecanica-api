using AutoGarage.Application.Services.Mapping;
using Mapster;
using MapsterMapper;

namespace AutoGarage.CommonTestUtilities.Mapper;

public class MapperBuilder
{
    public static IMapper Build()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(EmployeeMappingConfig).Assembly);

        return new MapsterMapper.Mapper(config);
    }
}
