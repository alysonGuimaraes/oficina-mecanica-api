using AutoGarage.Application.Commands.Auth;
using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Communication.Requests.Auth;
using AutoGarage.Communication.Requests.Customers;
using AutoGarage.Communication.Requests.Employees;
using AutoGarage.Communication.Requests.Vehicles;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class RequestToCommandMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LoginRequest, LoginCommand>();
        config.NewConfig<CreateCustomerRequest, CreateCustomerCommand>();
        config.NewConfig<CreateVehicleRequest, CreateVehicleCommand>();
        config.NewConfig<CreateEmployeeRequest, CreateEmployeeCommand>();
    }
}
