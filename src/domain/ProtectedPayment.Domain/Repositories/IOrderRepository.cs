using ProtectedPayment.Domain.Entities;

namespace ProtectedPayment.Domain.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken);
    Task<Order?> GetOrderAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task UpdateAsync(Order order);
}
