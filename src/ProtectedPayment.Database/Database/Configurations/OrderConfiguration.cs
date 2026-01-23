using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtectedPayment.Database.Entities;

namespace ProtectedPayment.Database.Database.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(255);

        builder.Property(o => o.ProductName).HasMaxLength(255);

        builder.Property(o => o.Amount).HasColumnType("decimal(18,2)");

        builder.Property(o => o.Status).HasConversion<string>();

        builder.Property(o => o.CreatedAt).IsRequired();

        builder.Property(o => o.PendingUntil).IsRequired();

        builder.Property(o => o.ConfirmedAt).IsRequired(false);

        builder.Property(o => o.CancelledAt).IsRequired(false);
    }
}
