using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.Infrastructure.Database;
using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Infrastructure.Repositories;

internal sealed class InboxMessageRepository(OrderDbContext dbContext) : IInboxMessageRepository
{
    public async Task<List<InboxMessage>> GetUnprocessedMessagesAsync(EventType eventType, CancellationToken cancellationToken)
    {
        return await dbContext
            .InboxMessages
            .Where(x => x.Type == $"{eventType}" && x.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(InboxMessage message, CancellationToken cancellationToken)
    {
        await dbContext.InboxMessages.AddAsync(message, cancellationToken);
    }

    public Task UpdateAsync(InboxMessage message)
    {
        dbContext.InboxMessages.Update(message);

        return Task.CompletedTask;
    }
}
