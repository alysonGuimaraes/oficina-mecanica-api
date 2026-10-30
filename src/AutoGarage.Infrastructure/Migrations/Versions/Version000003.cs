using FluentMigrator;
using System.Data;

namespace AutoGarage.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_VEHICLE, "Create table to save the vehicle's information")]
public class Version000003 : VersionBase
{
    public override void Up()
    {
        CreateTable("vehicles")
            .WithColumn("Brand").AsString(50).NotNullable()
            .WithColumn("Model").AsString(50).NotNullable()
            .WithColumn("Plate").AsString(7).NotNullable()
            .WithColumn("PlateType")
                .AsInt32()
                .NotNullable()
                .WithColumnDescription("Enum: 1-OLD, 2-MERCOSUL")
            .WithColumn("Year").AsInt32().NotNullable()
            .WithColumn("VehicleType")
                .AsInt32()
                .NotNullable()
                .WithColumnDescription("Enum: 1-CAR, 2-MOTORCYCLE, 3-PICKUP, 4-MINIBUS, 5-VAN, 6-OTHER")
            .WithColumn("CustomerId").AsGuid().NotNullable();

        Create.Index("IX_vehicles_Plate")
            .OnTable("vehicles")
            .OnColumn("Plate").Ascending()
            .WithOptions().Unique();

        Create.Index("IX_vehicles_CustomerId")
            .OnTable("vehicles")
            .OnColumn("CustomerId").Ascending();

        Create.ForeignKey("FK_vehicles_customers")
            .FromTable("vehicles").ForeignColumn("CustomerId")
            .ToTable("customers").PrimaryColumn("Id")
            .OnDelete(Rule.None);
    }
}
