using FluentMigrator;

namespace AutoGarage.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_CUSTOMER, "Create table to save the customer's information")]
public class Version000002 : VersionBase
{
    public override void Up()
    {
        CreateTable("customers")
            .WithColumn("Name").AsString(150).NotNullable()
            .WithColumn("Document").AsString(14).NotNullable()
            .WithColumn("DocumentType")
                .AsInt32()
                .NotNullable()
                .WithColumnDescription("Enum: 1-CPF, 2-CNPJ")
            .WithColumn("Email").AsString(150).NotNullable()
            .WithColumn("Ddd").AsString(2).NotNullable()
            .WithColumn("PhoneNumber").AsString(9).NotNullable();

        Create.Index("IX_customers_Document")
            .OnTable("customers")
            .OnColumn("Document").Ascending()
            .WithOptions().Unique();

        Create.Index("IX_customers_Email")
            .OnTable("customers")
            .OnColumn("Email").Ascending()
            .WithOptions().Unique();
    }
}
