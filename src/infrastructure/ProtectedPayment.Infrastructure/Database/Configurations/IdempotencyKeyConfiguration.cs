using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtectedPayment.Domain.Entities;

namespace ProtectedPayment.Infrastructure.Database.Configurations;

public sealed class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_keys");

        builder.HasKey(o => o.Key);

        builder.Property(o => o.EventType).HasConversion<int>();
    }
}
