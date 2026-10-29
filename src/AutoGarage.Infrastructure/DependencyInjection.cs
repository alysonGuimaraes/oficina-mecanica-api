using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Employees;
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

        AddRepositories(services);

        AddDbContext(services, configuration);

        AddFluentMigrator_Postgres(services, configuration);

        return services;
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
