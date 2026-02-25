using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.Infrastructure.Database;

namespace ProtectedPayment.Infrastructure.Repositories;

internal sealed class OrderRepository(OrderDbContext dbContext) : IOrderRepository
{
    public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        return await dbContext
            .Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext
            .Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public Task UpdateAsync(Order order)
    {
        dbContext.Orders.Update(order);

        return Task.CompletedTask;
    }
}
