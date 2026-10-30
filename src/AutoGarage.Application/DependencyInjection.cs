using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Customers.CreateCustomer;
using AutoGarage.Application.UseCases.Customers.GetCustomerByDocument;
using AutoGarage.Application.UseCases.Customers.GetCustomerById;
using AutoGarage.Application.UseCases.Customers.ListCustomers;
using AutoGarage.Application.UseCases.Customers.UpdateCustomer;
using AutoGarage.Application.UseCases.Customers.UpdateCustomerStatus;
using AutoGarage.Application.UseCases.Vehicles.CreateVehicle;
using AutoGarage.Application.UseCases.Vehicles.GetVehicleById;
using AutoGarage.Application.UseCases.Vehicles.ListVehicles;
using AutoGarage.Application.UseCases.Vehicles.TransferVehicleOwner;
using AutoGarage.Application.UseCases.Vehicles.UpdateVehicle;
using AutoGarage.Application.UseCases.Vehicles.UpdateVehicleStatus;
using AutoGarage.Application.UseCases.Auth.Login;
using AutoGarage.Application.UseCases.Employees.CreateEmployee;
using AutoGarage.Application.UseCases.Employees.GetEmployeeById;
using AutoGarage.Application.UseCases.Employees.ListEmployees;
using AutoGarage.Application.UseCases.Employees.UpdateEmployee;
using AutoGarage.Application.UseCases.Employees.UpdateEmployeeStatus;
using AutoGarage.Application.UseCases.Services.CreateService;
using AutoGarage.Application.UseCases.Services.GetServiceById;
using AutoGarage.Application.UseCases.Services.ListServices;
using AutoGarage.Application.UseCases.Services.UpdateService;
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

        services.AddSingleton<IStringNormalizer, StringNormalizer>();

        AddMapsterConfig(services);

        return services;
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<ICreateEmployeeUseCase, CreateEmployeeUseCase>();
        services.AddScoped<IListEmployeesUseCase, ListEmployeesUseCase>();
        services.AddScoped<IGetEmployeeByIdUseCase, GetEmployeeByIdUseCase>();
        services.AddScoped<IUpdateEmployeeUseCase, UpdateEmployeeUseCase>();
        services.AddScoped<IUpdateEmployeeStatusUseCase, UpdateEmployeeStatusUseCase>();
        services.AddScoped<ICreateCustomerUseCase, CreateCustomerUseCase>();
        services.AddScoped<IListCustomersUseCase, ListCustomersUseCase>();
        services.AddScoped<IGetCustomerByIdUseCase, GetCustomerByIdUseCase>();
        services.AddScoped<IGetCustomerByDocumentUseCase, GetCustomerByDocumentUseCase>();
        services.AddScoped<IUpdateCustomerUseCase, UpdateCustomerUseCase>();
        services.AddScoped<IUpdateCustomerStatusUseCase, UpdateCustomerStatusUseCase>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<ICreateVehicleUseCase, CreateVehicleUseCase>();
        services.AddScoped<IListVehiclesUseCase, ListVehiclesUseCase>();
        services.AddScoped<IGetVehicleByIdUseCase, GetVehicleByIdUseCase>();
        services.AddScoped<IUpdateVehicleUseCase, UpdateVehicleUseCase>();
        services.AddScoped<ITransferVehicleOwnerUseCase, TransferVehicleOwnerUseCase>();
        services.AddScoped<IUpdateVehicleStatusUseCase, UpdateVehicleStatusUseCase>();
        services.AddScoped<ICreateServiceUseCase, CreateServiceUseCase>();
        services.AddScoped<IListServicesUseCase, ListServicesUseCase>();
        services.AddScoped<IGetServiceByIdUseCase, GetServiceByIdUseCase>();
        services.AddScoped<IUpdateServiceUseCase, UpdateServiceUseCase>();
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
