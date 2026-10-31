using FluentMigrator;
using System.Data;

namespace AutoGarage.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_SERVICE_ORDER, "Create tables to save service orders, their items and status history")]
public class Version000007 : VersionBase
{
    public override void Up()
    {
        Create.Table("service_orders")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("CustomerId").AsGuid().NotNullable()
            .WithColumn("VehicleId").AsGuid().NotNullable()
            .WithColumn("Status")
                .AsInt32()
                .NotNullable()
                .WithColumnDescription("Enum: 1-RECEIVED, 2-IN_DIAGNOSIS, 3-AWAITING_APPROVAL, 4-AWAITING_MATERIALS, 5-IN_EXECUTION, 6-FINISHED, 7-CANCELLED, 8-DELIVERED")
            .WithColumn("BudgetTotal").AsDecimal(10, 2).Nullable()
            .WithColumn("BudgetSentAt").AsCustom("timestamptz").Nullable()
            .WithColumn("Observation").AsString(500).Nullable()
            .WithColumn("CancellationReason").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsCustom("timestamptz").NotNullable()
            .WithColumn("UpdatedAt").AsCustom("timestamptz").Nullable();

        Create.ForeignKey("FK_service_orders_customers")
            .FromTable("service_orders").ForeignColumn("CustomerId")
            .ToTable("customers").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.ForeignKey("FK_service_orders_vehicles")
            .FromTable("service_orders").ForeignColumn("VehicleId")
            .ToTable("vehicles").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.Index("IX_service_orders_CustomerId")
            .OnTable("service_orders")
            .OnColumn("CustomerId").Ascending();

        Create.Index("IX_service_orders_VehicleId")
            .OnTable("service_orders")
            .OnColumn("VehicleId").Ascending();

        Create.Index("IX_service_orders_Status")
            .OnTable("service_orders")
            .OnColumn("Status").Ascending();

        Execute.Sql("ALTER TABLE service_orders ADD CONSTRAINT \"CK_service_orders_BudgetTotal\" CHECK (\"BudgetTotal\" >= 0);");

        CreateTable("service_order_services")
            .WithColumn("ServiceOrderId").AsGuid().NotNullable()
            .WithColumn("ServiceId").AsGuid().NotNullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable();

        Create.ForeignKey("FK_service_order_services_service_orders")
            .FromTable("service_order_services").ForeignColumn("ServiceOrderId")
            .ToTable("service_orders").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.ForeignKey("FK_service_order_services_services")
            .FromTable("service_order_services").ForeignColumn("ServiceId")
            .ToTable("services").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.Index("IX_service_order_services_ServiceOrderId")
            .OnTable("service_order_services")
            .OnColumn("ServiceOrderId").Ascending();

        Create.Index("IX_service_order_services_ServiceOrderId_ServiceId")
            .OnTable("service_order_services")
            .OnColumn("ServiceOrderId").Ascending()
            .OnColumn("ServiceId").Ascending()
            .WithOptions().Unique();

        Execute.Sql("ALTER TABLE service_order_services ADD CONSTRAINT \"CK_service_order_services_Price\" CHECK (\"Price\" > 0);");

        CreateTable("service_order_materials")
            .WithColumn("ServiceOrderId").AsGuid().NotNullable()
            .WithColumn("MaterialId").AsGuid().NotNullable()
            .WithColumn("Quantity").AsInt32().NotNullable()
            .WithColumn("UnitPrice").AsDecimal(10, 2).NotNullable();

        Create.ForeignKey("FK_service_order_materials_service_orders")
            .FromTable("service_order_materials").ForeignColumn("ServiceOrderId")
            .ToTable("service_orders").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.ForeignKey("FK_service_order_materials_materials")
            .FromTable("service_order_materials").ForeignColumn("MaterialId")
            .ToTable("materials").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.Index("IX_service_order_materials_ServiceOrderId")
            .OnTable("service_order_materials")
            .OnColumn("ServiceOrderId").Ascending();

        Create.Index("IX_service_order_materials_ServiceOrderId_MaterialId")
            .OnTable("service_order_materials")
            .OnColumn("ServiceOrderId").Ascending()
            .OnColumn("MaterialId").Ascending()
            .WithOptions().Unique();

        Execute.Sql("ALTER TABLE service_order_materials ADD CONSTRAINT \"CK_service_order_materials_Quantity\" CHECK (\"Quantity\" > 0);");
        Execute.Sql("ALTER TABLE service_order_materials ADD CONSTRAINT \"CK_service_order_materials_UnitPrice\" CHECK (\"UnitPrice\" >= 0);");

        Create.Table("service_order_status_history")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("ServiceOrderId").AsGuid().NotNullable()
            .WithColumn("EventType")
                .AsInt32()
                .NotNullable()
                .WithColumnDescription("Enum: 1-STATUS_CHANGE, 2-ITEM_SERVICE, 3-ITEM_MATERIAL")
            .WithColumn("FromStatus").AsInt32().Nullable()
            .WithColumn("ToStatus").AsInt32().Nullable()
            .WithColumn("ItemId").AsGuid().Nullable()
            .WithColumn("ItemDirection")
                .AsInt32()
                .Nullable()
                .WithColumnDescription("Enum: 1-IN, 2-OUT")
            .WithColumn("EmployeeId").AsGuid().NotNullable()
            .WithColumn("CreatedAt").AsCustom("timestamptz").NotNullable();

        Create.ForeignKey("FK_service_order_status_history_service_orders")
            .FromTable("service_order_status_history").ForeignColumn("ServiceOrderId")
            .ToTable("service_orders").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.ForeignKey("FK_service_order_status_history_employees")
            .FromTable("service_order_status_history").ForeignColumn("EmployeeId")
            .ToTable("employees").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.Index("IX_service_order_status_history_ServiceOrderId")
            .OnTable("service_order_status_history")
            .OnColumn("ServiceOrderId").Ascending();

        Execute.Sql("ALTER TABLE service_order_status_history ADD CONSTRAINT \"CK_service_order_status_history_Item\" CHECK ((\"ItemId\" IS NULL) = (\"ItemDirection\" IS NULL));");

        Alter.Table("material_transactions")
            .AddColumn("ServiceOrderId").AsGuid().Nullable();

        Create.ForeignKey("FK_material_transactions_service_orders")
            .FromTable("material_transactions").ForeignColumn("ServiceOrderId")
            .ToTable("service_orders").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.Index("IX_material_transactions_ServiceOrderId")
            .OnTable("material_transactions")
            .OnColumn("ServiceOrderId").Ascending();
    }
}
