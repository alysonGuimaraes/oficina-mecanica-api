using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Requests.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Entities;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class ServiceOrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateServiceOrderRequest, CreateServiceOrderCommand>();

        config.NewConfig<ServiceOrder, ServiceOrderResponse>();
    }
}
