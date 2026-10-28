using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AutoGarage.Infrastructure.Migrations;

public static class DatabaseMigrations
{
    public static void Migrate(string connectionString, IServiceProvider serviceProvider)
    {
        EnsureDatabaseCreatedPostgres(connectionString);

        MigrationDatabase(serviceProvider);
    }

    private static void EnsureDatabaseCreatedPostgres(string connectionString)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var dataBaseName = connectionStringBuilder.Database;

        connectionStringBuilder.Remove("Database");

        using var dbConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

        var parameters = new DynamicParameters();
        parameters.Add("name", dataBaseName);

        var exists = dbConnection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @name)",
            parameters);

        if (!exists)
        {
            dbConnection.Execute($"CREATE DATABASE {dataBaseName}");
        }
    }

    private static void MigrationDatabase(IServiceProvider serviceProvider)
    {
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        runner.ListMigrations();

        runner.MigrateUp();
    }
}
