using AutoGarage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoGarage.Infrastructure.Persistence.Configurations;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("materials");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(m => m.Name)
            .IsUnique();

        builder.Property(m => m.Description)
            .HasMaxLength(500);

        builder.Property(m => m.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(m => m.StockQuantity)
            .IsRequired();

        builder.Property(m => m.StockMinimum)
            .IsRequired();

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);
    }
}
