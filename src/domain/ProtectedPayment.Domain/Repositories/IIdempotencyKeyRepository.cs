using ProtectedPayment.Domain.Entities;
using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Domain.Repositories;

public interface IIdempotencyKeyRepository
{
    Task<bool> HasIdempotencyKey(Guid idempotencyKey, EventType eventType, CancellationToken cancellationToken);
    Task AddAsync(IdempotencyKey message, CancellationToken cancellationToken);
}
