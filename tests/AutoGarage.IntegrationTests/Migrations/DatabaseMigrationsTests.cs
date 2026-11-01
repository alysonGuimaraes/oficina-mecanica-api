using AutoGarage.Infrastructure.Migrations;
using Npgsql;
using Shouldly;

namespace AutoGarage.IntegrationTests.Migrations;

public class DatabaseMigrationsTests
{
    [Fact]
    public void BuildServerConnectionString_ShouldTargetPostgresDatabase_KeepingHostAndCredentials()
    {
        var result = DatabaseMigrations.BuildServerConnectionString(
            "Host=db.internal;Port=5432;Database=autogarage;Username=autogarage_admin;Password=secret");

        var builder = new NpgsqlConnectionStringBuilder(result);
        builder.Database.ShouldBe("postgres");
        builder.Host.ShouldBe("db.internal");
        builder.Username.ShouldBe("autogarage_admin");
        builder.Password.ShouldBe("secret");
    }

    [Fact]
    public void BuildServerConnectionString_WithoutDatabase_ShouldNotFallBackToUsernameDatabase()
    {
        var result = DatabaseMigrations.BuildServerConnectionString(
            "Host=db.internal;Username=autogarage_admin;Password=secret");

        new NpgsqlConnectionStringBuilder(result).Database.ShouldBe("postgres");
    }
}
