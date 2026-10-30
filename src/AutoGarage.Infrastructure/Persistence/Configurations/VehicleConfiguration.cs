using AutoGarage.Domain.Entities;
using AutoGarage.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoGarage.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Brand)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Plate)
            .HasConversion(plate => plate.Value, value => new Plate(value))
            .IsRequired()
            .HasMaxLength(7);

        builder.HasIndex(v => v.Plate)
            .IsUnique();

        builder.Property(v => v.PlateType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.Year)
            .IsRequired();

        builder.Property(v => v.VehicleType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.CustomerId)
            .IsRequired();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt);
    }
}
