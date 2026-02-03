using Confluent.Kafka;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Worker.Services;

public sealed class KafkaStateService
{
    private readonly Dictionary<Guid, OrderPendingHistory> _pendingMessages = new();

    public IReadOnlyDictionary<Guid, OrderPendingHistory> PendingMessages => _pendingMessages;

    public void AddOrderPendingMessage(Guid orderId, OrderPendingHistory history)
    {
        _pendingMessages[orderId] = history;
    }

    public void RemoveOrderPendingMessage(Guid orderId)
    {
        _pendingMessages.Remove(orderId);
    }
}

public record OrderPendingHistory(OrderPendingEvent Event, TopicPartitionOffset Offset, Guid IdempotencyKey, EventType EventType);
