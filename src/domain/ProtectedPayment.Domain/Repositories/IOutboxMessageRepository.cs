using ProtectedPayment.Domain.Entities;

namespace ProtectedPayment.Domain.Repositories;

public interface IOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken);
    Task UpdateAsync(OutboxMessage message);
}
