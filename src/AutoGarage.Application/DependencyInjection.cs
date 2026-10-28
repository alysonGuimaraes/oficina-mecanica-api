using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Application.UseCases.Employees.CreateEmployee;
using AutoGarage.Application.UseCases.Employees.GetEmployeeById;
using AutoGarage.Application.UseCases.Employees.ListEmployees;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AutoGarage.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {

        AddUseCases(services);

        AddPasswordEncripter(services, configuration);

        AddMapsterConfig(services);

        return services;
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<ICreateEmployeeUseCase, CreateEmployeeUseCase>();
        services.AddScoped<IListEmployeesUseCase, ListEmployeesUseCase>();
        services.AddScoped<IGetEmployeeByIdUseCase, GetEmployeeByIdUseCase>();
    }

    private static void AddPasswordEncripter(IServiceCollection services, IConfiguration configuration)
    {
        var additionalKey = configuration["Settings:Password:AdditionalKey"];
        services.AddScoped(option => new PasswordEncripter(additionalKey!));
    }

    private static void AddMapsterConfig(IServiceCollection services)
    {
        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();
    }
}
