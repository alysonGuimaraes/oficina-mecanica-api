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

    // Connection string for the server-level database used to check and create the target database.
    // Targets "postgres" explicitly: without a Database key, Npgsql falls back to the username,
    // which on Cloud SQL (user autogarage_admin) is not an existing database.
    public static string BuildServerConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres"
        };

        return builder.ConnectionString;
    }

    private static void EnsureDatabaseCreatedPostgres(string connectionString)
    {
        var dataBaseName = new NpgsqlConnectionStringBuilder(connectionString).Database;

        using var dbConnection = new NpgsqlConnection(BuildServerConnectionString(connectionString));

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
