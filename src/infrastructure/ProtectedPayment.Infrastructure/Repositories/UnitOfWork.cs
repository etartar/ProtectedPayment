using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.Infrastructure.Database;

namespace ProtectedPayment.Infrastructure.Repositories;

internal sealed class UnitOfWork(OrderDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
