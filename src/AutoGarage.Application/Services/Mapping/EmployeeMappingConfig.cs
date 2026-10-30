using AutoGarage.Application.Commands.Employees;
using AutoGarage.Communication.Requests.Employees;
using AutoGarage.Communication.Responses.Employees;
using AutoGarage.Domain.Entities;
using Mapster;

namespace AutoGarage.Application.Services.Mapping;

public class EmployeeMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateEmployeeRequest, Employee>();
        config.NewConfig<CreateEmployeeRequest, CreateEmployeeCommand>();

        config.NewConfig<Employee, EmployeeResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Role, src => src.Role)
            .Map(dest => dest.Status, src => src.Status);
    }
}
