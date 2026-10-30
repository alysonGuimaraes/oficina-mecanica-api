using AutoGarage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoGarage.Infrastructure.Persistence.Configurations;

public class MaterialTransactionConfiguration : IEntityTypeConfiguration<MaterialTransaction>
{
    public void Configure(EntityTypeBuilder<MaterialTransaction> builder)
    {
        builder.ToTable("material_transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.MaterialId)
            .IsRequired();

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Quantity)
            .IsRequired();

        builder.Property(t => t.BalanceAfter)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasOne<Material>()
            .WithMany()
            .HasForeignKey(t => t.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
