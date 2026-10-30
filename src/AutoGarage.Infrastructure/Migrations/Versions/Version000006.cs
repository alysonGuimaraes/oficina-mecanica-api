using FluentMigrator;
using System.Data;

namespace AutoGarage.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_MATERIAL_TRANSACTION, "Create table to save the material's stock transactions")]
public class Version000006 : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("material_transactions")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("MaterialId").AsGuid().NotNullable()
            .WithColumn("Type")
                .AsInt32()
                .NotNullable()
                .WithColumnDescription("Enum: 1-IN, 2-OUT")
            .WithColumn("Quantity").AsInt32().NotNullable()
            .WithColumn("BalanceAfter").AsInt32().NotNullable()
            .WithColumn("CreatedAt").AsCustom("timestamptz").NotNullable();

        Create.ForeignKey("FK_material_transactions_materials")
            .FromTable("material_transactions").ForeignColumn("MaterialId")
            .ToTable("materials").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.Index("IX_material_transactions_MaterialId")
            .OnTable("material_transactions")
            .OnColumn("MaterialId").Ascending();

        Execute.Sql("ALTER TABLE material_transactions ADD CONSTRAINT \"CK_material_transactions_Quantity\" CHECK (\"Quantity\" > 0);");
        Execute.Sql("ALTER TABLE material_transactions ADD CONSTRAINT \"CK_material_transactions_BalanceAfter\" CHECK (\"BalanceAfter\" >= 0);");
    }
}
