using AutoGarage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoGarage.Infrastructure.Persistence.Configurations;

public class ServiceOrderServiceConfiguration : IEntityTypeConfiguration<ServiceOrderService>
{
    public void Configure(EntityTypeBuilder<ServiceOrderService> builder)
    {
        builder.ToTable("service_order_services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ServiceOrderId)
            .IsRequired();

        builder.HasIndex(s => s.ServiceOrderId);

        builder.Property(s => s.ServiceId)
            .IsRequired();

        builder.HasIndex(s => new { s.ServiceOrderId, s.ServiceId })
            .IsUnique();

        builder.Property(s => s.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.HasOne<ServiceOrder>()
            .WithMany()
            .HasForeignKey(s => s.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Service>()
            .WithMany()
            .HasForeignKey(s => s.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
