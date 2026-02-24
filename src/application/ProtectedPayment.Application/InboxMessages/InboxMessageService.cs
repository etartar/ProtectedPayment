using ProtectedPayment.Application.Contracts.InboxMessages;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Application.InboxMessages;

public class InboxMessageService(
    IIdempotencyKeyRepository idempotencyKeyRepository,
    IInboxMessageRepository inboxMessageRepository,
    IUnitOfWork unitOfWork) : IInboxMessageService
{
    public async Task<bool> HasIdempotencyKey(Guid idempotencyKey, EventType eventType, CancellationToken cancellationToken = default)
    {
        return await idempotencyKeyRepository.HasIdempotencyKey(idempotencyKey, eventType, cancellationToken);
    }

    public async Task SaveInboxMessage(Guid idempotencyKey, EventType eventType, string eventData, CancellationToken cancellationToken = default)
    {
        var inbox = new InboxMessage
        {
            Id = Guid.CreateVersion7(),
            Type = $"{eventType}",
            Content = eventData,
            OccurredOnUtc = DateTime.UtcNow
        };

        await inboxMessageRepository.AddAsync(inbox, cancellationToken);

        await idempotencyKeyRepository.AddAsync(new IdempotencyKey
        {
            Key = idempotencyKey,
            EventType = eventType,
            Created = DateTime.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
