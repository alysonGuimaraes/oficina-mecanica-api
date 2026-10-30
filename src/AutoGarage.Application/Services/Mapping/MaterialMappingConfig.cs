using AutoGarage.Application.Commands.Materials;
using AutoGarage.Communication.Requests.Materials;
using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Entities;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class MaterialMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateMaterialRequest, CreateMaterialCommand>();

        config.NewConfig<Material, MaterialResponse>();
    }
}
