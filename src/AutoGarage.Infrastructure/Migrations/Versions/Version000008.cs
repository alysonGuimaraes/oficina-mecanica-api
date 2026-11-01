using FluentMigrator;

namespace AutoGarage.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.ALTER_SERVICE_ORDER_STATUS_HISTORY_CUSTOMER_ACTOR, "Allow status history without employee and flag customer actions")]
public class Version000008 : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Column("EmployeeId")
            .OnTable("service_order_status_history")
            .AsGuid()
            .Nullable();

        Alter.Table("service_order_status_history")
            .AddColumn("IsCustomerAction")
            .AsBoolean()
            .NotNullable()
            .WithDefaultValue(false);
    }
}
