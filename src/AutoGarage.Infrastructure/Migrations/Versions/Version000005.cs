using FluentMigrator;

namespace AutoGarage.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_MATERIAL, "Create table to save the material's information")]
public class Version000005 : VersionBase
{
    public override void Up()
    {
        CreateTable("materials")
            .WithColumn("Name").AsString(150).NotNullable()
            .WithColumn("Description").AsString(500).Nullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable()
            .WithColumn("StockQuantity").AsInt32().NotNullable()
            .WithColumn("StockMinimum").AsInt32().NotNullable();

        Create.Index("IX_materials_Name")
            .OnTable("materials")
            .OnColumn("Name").Ascending()
            .WithOptions().Unique();

        Execute.Sql("ALTER TABLE materials ADD CONSTRAINT \"CK_materials_Price\" CHECK (\"Price\" >= 0);");
        Execute.Sql("ALTER TABLE materials ADD CONSTRAINT \"CK_materials_StockQuantity\" CHECK (\"StockQuantity\" >= 0);");
        Execute.Sql("ALTER TABLE materials ADD CONSTRAINT \"CK_materials_StockMinimum\" CHECK (\"StockMinimum\" >= 0);");
    }
}
