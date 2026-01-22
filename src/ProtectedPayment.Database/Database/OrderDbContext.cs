using Microsoft.EntityFrameworkCore;

namespace ProtectedPayment.Database.Database;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Entities.Order> Orders { get; set; }
    public DbSet<Entities.OutboxMessage> OutboxMessages { get; set; }
    public DbSet<Entities.InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
