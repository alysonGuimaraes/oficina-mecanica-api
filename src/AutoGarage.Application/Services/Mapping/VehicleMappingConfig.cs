using AutoGarage.Communication.Responses.Vehicles;
using AutoGarage.Domain.Entities;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class VehicleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Vehicle, VehicleResponse>()
            .Map(dest => dest.Plate, src => src.Plate.Value);
    }
}
