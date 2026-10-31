using AutoGarage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoGarage.Infrastructure.Persistence.Configurations;

public class ServiceOrderMaterialConfiguration : IEntityTypeConfiguration<ServiceOrderMaterial>
{
    public void Configure(EntityTypeBuilder<ServiceOrderMaterial> builder)
    {
        builder.ToTable("service_order_materials");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ServiceOrderId)
            .IsRequired();

        builder.HasIndex(m => m.ServiceOrderId);

        builder.Property(m => m.MaterialId)
            .IsRequired();

        builder.HasIndex(m => new { m.ServiceOrderId, m.MaterialId })
            .IsUnique();

        builder.Property(m => m.Quantity)
            .IsRequired();

        builder.Property(m => m.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);

        builder.HasOne<ServiceOrder>()
            .WithMany()
            .HasForeignKey(m => m.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Material>()
            .WithMany()
            .HasForeignKey(m => m.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
