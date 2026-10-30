using AutoGarage.Application.Commands.Services;
using AutoGarage.Communication.Requests.Services;
using AutoGarage.Communication.Responses.Services;
using AutoGarage.Domain.Entities;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class ServiceMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateServiceRequest, CreateServiceCommand>();

        config.NewConfig<Service, ServiceResponse>();
    }
}
