using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Worker.Services;

public sealed class RedisStateService
{
    private readonly Dictionary<Guid, OrderPendingRedisHistory> _pendingMessages = new();

    public IReadOnlyDictionary<Guid, OrderPendingRedisHistory> PendingMessages => _pendingMessages;

    public void AddOrderPendingMessage(Guid orderId, OrderPendingRedisHistory history)
    {
        _pendingMessages[orderId] = history;
    }

    public void RemoveOrderPendingMessage(Guid orderId)
    {
        _pendingMessages.Remove(orderId);
    }
}

public record OrderPendingRedisHistory(OrderPendingEvent Event, Guid IdempotencyKey, EventType EventType);
