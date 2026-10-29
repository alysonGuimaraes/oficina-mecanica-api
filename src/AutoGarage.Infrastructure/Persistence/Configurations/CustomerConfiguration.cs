using AutoGarage.Domain.Entities;
using AutoGarage.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoGarage.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Document)
            .HasConversion(document => document.Value, value => new Document(value))
            .IsRequired()
            .HasMaxLength(14);

        builder.HasIndex(c => c.Document)
            .IsUnique();

        builder.Property(c => c.DocumentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Ddd)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(9);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);
    }
}
