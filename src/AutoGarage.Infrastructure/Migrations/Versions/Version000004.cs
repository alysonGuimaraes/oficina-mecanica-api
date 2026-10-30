using FluentMigrator;

namespace AutoGarage.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_SERVICE, "Create table to save the service's information")]
public class Version000004 : VersionBase
{
    public override void Up()
    {
        CreateTable("services")
            .WithColumn("Name").AsString(150).NotNullable()
            .WithColumn("Description").AsString(500).Nullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable();

        Create.Index("IX_services_Name")
            .OnTable("services")
            .OnColumn("Name").Ascending()
            .WithOptions().Unique();

        Execute.Sql("ALTER TABLE services ADD CONSTRAINT \"CK_services_Price\" CHECK (\"Price\" > 0);");
    }
}
