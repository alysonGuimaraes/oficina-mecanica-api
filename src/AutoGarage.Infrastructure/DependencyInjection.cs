using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Repositories.Vehicles;
using AutoGarage.Application.Services.Authentication;
using AutoGarage.Domain.Repositories.Employees;
using AutoGarage.Domain.Repositories.Services;
using AutoGarage.Domain.Repositories.Materials;
using AutoGarage.Domain.Repositories.MaterialTransactions;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using AutoGarage.Infrastructure.Authentication;
using AutoGarage.Application.Services.Notifications;
using AutoGarage.Infrastructure.Notifications;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AutoGarage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        AddAuthentication(services, configuration);

        AddNotifications(services, configuration);

        AddRepositories(services);

        AddDbContext(services, configuration);

        AddFluentMigrator_Postgres(services, configuration);

        return services;
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Settings:Jwt");

        var settings = new JwtSettings
        {
            Key = section["Key"] ?? string.Empty,
            Issuer = section["Issuer"] ?? string.Empty,
            Audience = section["Audience"] ?? string.Empty,
            ExpiresInMinutes = int.Parse(section["ExpiresInMinutes"] ?? "60")
        };

        if (string.IsNullOrWhiteSpace(settings.Key))
        {
            throw new InvalidOperationException("Settings:Jwt:Key must be configured.");
        }

        services.AddSingleton<IAccessTokenGenerator>(new JwtAccessTokenGenerator(settings));
    }

    private static void AddNotifications(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Settings:Brevo");

        var settings = new BrevoSettings
        {
            ApiKey = section["ApiKey"] ?? string.Empty,
            FromEmail = section["FromEmail"] ?? string.Empty,
            FromName = section["FromName"] ?? string.Empty,
            Enabled = bool.TryParse(section["Enabled"], out var enabled) && enabled
        };

        if (!settings.Enabled)
        {
            services.AddScoped<IEmailSender, LogOnlyEmailSender>();
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.FromEmail))
        {
            throw new InvalidOperationException("Settings:Brevo:ApiKey and Settings:Brevo:FromEmail must be configured when Settings:Brevo:Enabled is true.");
        }

        services.AddSingleton(settings);
        services.AddHttpClient<IEmailSender, BrevoEmailSender>(client => client.Timeout = TimeSpan.FromSeconds(5));
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(dbContextOptions =>
        {
            dbContextOptions.UseNpgsql(connectionString);
        });
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitWork, WorkUnit>();

        services.AddScoped<IEmployeeWriteOnlyRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeReadOnlyRepository, EmployeeRepository>();

        services.AddScoped<ICustomerWriteOnlyRepository, CustomerRepository>();
        services.AddScoped<ICustomerReadOnlyRepository, CustomerRepository>();
        services.AddScoped<IVehicleWriteOnlyRepository, VehicleRepository>();
        services.AddScoped<IVehicleReadOnlyRepository, VehicleRepository>();

        services.AddScoped<IServiceWriteOnlyRepository, ServiceRepository>();
        services.AddScoped<IServiceReadOnlyRepository, ServiceRepository>();

        services.AddScoped<IMaterialWriteOnlyRepository, MaterialRepository>();
        services.AddScoped<IMaterialReadOnlyRepository, MaterialRepository>();
        services.AddScoped<IMaterialTransactionWriteOnlyRepository, MaterialTransactionRepository>();
        services.AddScoped<IMaterialTransactionReadOnlyRepository, MaterialTransactionRepository>();

        services.AddScoped<IServiceOrderReadOnlyRepository, ServiceOrderRepository>();
        services.AddScoped<IServiceOrderWriteOnlyRepository, ServiceOrderRepository>();
        services.AddScoped<IServiceOrderServiceReadOnlyRepository, ServiceOrderServiceRepository>();
        services.AddScoped<IServiceOrderServiceWriteOnlyRepository, ServiceOrderServiceRepository>();
        services.AddScoped<IServiceOrderMaterialReadOnlyRepository, ServiceOrderMaterialRepository>();
        services.AddScoped<IServiceOrderMaterialWriteOnlyRepository, ServiceOrderMaterialRepository>();
        services.AddScoped<IServiceOrderStatusHistoryReadOnlyRepository, ServiceOrderStatusHistoryRepository>();
        services.AddScoped<IServiceOrderStatusHistoryWriteOnlyRepository, ServiceOrderStatusHistoryRepository>();
    }

    private static void AddFluentMigrator_Postgres(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddFluentMigratorCore().ConfigureRunner(options =>
        {
            options
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(Assembly.Load("AutoGarage.Infrastructure")).For.All();
        });
    }
}
