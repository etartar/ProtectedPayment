using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Domain.Entities;

namespace ProtectedPayment.Infrastructure.Database;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
