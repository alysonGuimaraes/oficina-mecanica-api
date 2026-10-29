using FluentMigrator;

namespace AutoGarage.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_EMPLOYEE, "Create table to save the employee's information")]
public class Version000001 : VersionBase
{
    public override void Up()
    {
        CreateTable("employees")
            .WithColumn("Name").AsString(150).NotNullable()
            .WithColumn("Email").AsString(150).NotNullable()
            .WithColumn("PasswordHash").AsString().NotNullable()
            .WithColumn("Role")
                .AsInt32()
                .NotNullable()
                .WithColumnDescription("Enum: 1-MECHANIC, 2-ATTENDANT");

        Create.Index("IX_employees_Email")
            .OnTable("employees")
            .OnColumn("Email").Ascending()
            .WithOptions().Unique();
    }
}
