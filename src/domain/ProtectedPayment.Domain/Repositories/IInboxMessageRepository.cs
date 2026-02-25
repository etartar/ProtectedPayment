using ProtectedPayment.Domain.Entities;
using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Domain.Repositories;

public interface IInboxMessageRepository
{
    Task<List<InboxMessage>> GetUnprocessedMessagesAsync(EventType eventType, CancellationToken cancellationToken);
    Task AddAsync(InboxMessage message, CancellationToken cancellationToken);
    Task UpdateAsync(InboxMessage message);
}
