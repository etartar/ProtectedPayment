using Newtonsoft.Json;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Serialization;
using StackExchange.Redis;

namespace ProtectedPayment.Worker.Services;

public sealed class RedisStateService
{
    private const string SCHEDULED_SET_KEY = "order:scheduled";
    private const string ORDER_PENDING_DATA = "orderPendingData:{0}";
    private readonly IDatabase _db;

    public RedisStateService(IRedisConnection redisConnection)
    {
        _db = redisConnection.GetDatabase();
    }

    public async Task<RedisValue[]> GetDueOrders()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return await _db.SortedSetRangeByScoreAsync(
            key: SCHEDULED_SET_KEY,
            start: 0,
            stop: now,
            take: 10);
    }

    public async Task<OrderPendingRedisHistory?> GetOrderPendingData(Guid orderId)
    {
        var data = await _db.StringGetAsync(string.Format(ORDER_PENDING_DATA, orderId));

        if (data.IsNullOrEmpty)
            return null;

        return JsonConvert.DeserializeObject<OrderPendingRedisHistory>(data!, SerializerSettings.Instance);
    }

    public async Task AddOrderPendingMessage(Guid orderId, DateTime scheduledTime, OrderPendingRedisHistory history)
    {
        // 1. Sorted Set'e ekle (scheduling için)
        // Score = Unix timestamp (saniye cinsinden)
        var score = new DateTimeOffset(scheduledTime).ToUnixTimeSeconds();

        await _db.SortedSetAddAsync(key: SCHEDULED_SET_KEY, member: orderId.ToString(), score: score);
        
        // 2. Event detaylarını ayrı key'de tut (hızlı erişim için)
        await _db.StringSetAsync(
            key: string.Format(ORDER_PENDING_DATA, orderId),
            value: JsonConvert.SerializeObject(history, SerializerSettings.Instance),
            expiry: TimeSpan.FromMinutes(15) // TTL
        );
    }

    public async Task RemoveOrderPendingMessage(Guid orderId)
    {
        // Sorted Set'ten sil (artık işlenmemeli)
        await _db.SortedSetRemoveAsync(SCHEDULED_SET_KEY, orderId.ToString());

        // Event'i sil
        await _db.KeyDeleteAsync(string.Format(ORDER_PENDING_DATA, orderId));
    }
}

public record OrderPendingRedisHistory(OrderPendingEvent Event, Guid IdempotencyKey, EventType EventType);
