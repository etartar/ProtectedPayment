using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.Infrastructure.Database;
using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Infrastructure.Repositories;

internal sealed class IdempotencyKeyRepository(OrderDbContext dbContext) : IIdempotencyKeyRepository
{
    public async Task<bool> HasIdempotencyKey(Guid idempotencyKey, EventType eventType, CancellationToken cancellationToken)
    {
        return await dbContext
            .IdempotencyKeys
            .AnyAsync(x => x.Key == idempotencyKey && x.EventType == eventType);
    }

    public async Task AddAsync(IdempotencyKey message, CancellationToken cancellationToken)
    {
        await dbContext.IdempotencyKeys.AddAsync(message, cancellationToken);
    }
}
