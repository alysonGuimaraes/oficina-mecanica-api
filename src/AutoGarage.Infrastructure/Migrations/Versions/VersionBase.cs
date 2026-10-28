using AutoGarage.Domain.Shared;
using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace AutoGarage.Infrastructure.Migrations.Versions;

public abstract class VersionBase : ForwardOnlyMigration
{
    protected ICreateTableColumnOptionOrWithColumnSyntax CreateTable(string table)
    {
        return Create.Table(table)
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("CreatedAt").AsCustom("timestamptz").NotNullable()
            .WithColumn("UpdatedAt").AsCustom("timestamptz").Nullable()
            .WithColumn("Status")
                .AsInt32()
                .NotNullable()
                .WithDefaultValue((int)EntityStatus.ACTIVE)
                .WithColumnDescription("Enum: 0-INACTIVE, 1-ACTIVE");
    }
}
