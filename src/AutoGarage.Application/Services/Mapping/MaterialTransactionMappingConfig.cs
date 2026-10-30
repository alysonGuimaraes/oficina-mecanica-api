using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Entities;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class MaterialTransactionMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MaterialTransaction, MaterialTransactionResponse>();
    }
}
