using Confluent.Kafka;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Worker.Services;

public sealed class KafkaService
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
