using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Database.Abstracts;
using ProtectedPayment.Database.Entities;
using System.Text.Json;

namespace Evently.Common.Infrastructure.Outbox;

public sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await InsertOutboxMessages(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task InsertOutboxMessages(DbContext context, CancellationToken cancellationToken)
    {
        var outboxMessages = context
            .ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                IReadOnlyCollection<IBaseEvent> events = entity.Events;

                entity.ClearEvents();

                return events;
            })
            .Select(baseEvent => new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                Type = baseEvent.GetType().Name,
                Content = JsonSerializer.Serialize(baseEvent),
                OccurredOnUtc = baseEvent.CreatedAt
            })
            .ToList();

        await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
    }
}
