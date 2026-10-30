using AutoGarage.Application.Commands.Customers;
using AutoGarage.Communication.Requests.Customers;
using AutoGarage.Communication.Responses.Customers;
using AutoGarage.Domain.Entities;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class CustomerMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCustomerRequest, CreateCustomerCommand>();

        config.NewConfig<Customer, CustomerResponse>()
            .Map(dest => dest.Document, src => src.Document.Value);
    }
}
