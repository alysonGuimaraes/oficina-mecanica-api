using AutoGarage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoGarage.Infrastructure.Persistence.Configurations;

public class ServiceOrderStatusHistoryConfiguration : IEntityTypeConfiguration<ServiceOrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<ServiceOrderStatusHistory> builder)
    {
        builder.ToTable("service_order_status_history");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ServiceOrderId)
            .IsRequired();

        builder.HasIndex(h => h.ServiceOrderId);

        builder.Property(h => h.EventType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.FromStatus)
            .HasConversion<int>();

        builder.Property(h => h.ToStatus)
            .HasConversion<int>();

        // Sem FK: o item pertence a uma de duas tabelas.
        builder.Property(h => h.ItemId);

        builder.Property(h => h.ItemDirection)
            .HasConversion<int>();

        builder.Property(h => h.EmployeeId)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.HasOne<ServiceOrder>()
            .WithMany()
            .HasForeignKey(h => h.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(h => h.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
