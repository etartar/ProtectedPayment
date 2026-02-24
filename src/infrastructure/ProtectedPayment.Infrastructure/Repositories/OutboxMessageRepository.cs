using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.Infrastructure.Database;

namespace ProtectedPayment.Infrastructure.Repositories;

internal sealed class OutboxMessageRepository(OrderDbContext dbContext) : IOutboxMessageRepository
{
    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken)
    {
        return await dbContext
            .OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(OutboxMessage message)
    {
        dbContext.OutboxMessages.Update(message);

        return Task.CompletedTask;
    }
}
