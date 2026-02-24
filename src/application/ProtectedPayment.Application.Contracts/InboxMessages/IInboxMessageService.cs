using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Application.Contracts.InboxMessages;

public interface IInboxMessageService
{
    Task<bool> HasIdempotencyKey(Guid idempotencyKey, EventType eventType, CancellationToken cancellationToken = default);
    Task SaveInboxMessage(Guid idempotencyKey, EventType eventType, string eventData, CancellationToken cancellationToken = default);
}
