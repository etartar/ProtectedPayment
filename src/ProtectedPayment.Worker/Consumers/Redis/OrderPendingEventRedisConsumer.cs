using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Worker.Services;
using StackExchange.Redis;

namespace ProtectedPayment.Worker.Consumers.Redis;

internal sealed class OrderPendingEventRedisConsumer : BaseRedisEventConsumer<OrderPendingEvent>
{
    private readonly RedisStateService _redisStateService;

    public OrderPendingEventRedisConsumer(
        ILogger<BaseRedisEventConsumer<OrderPendingEvent>> logger,
        IRedisConnection redisConnection,
        IServiceProvider serviceProvider,
        RedisStateService redisStateService) : base(logger, redisConnection, serviceProvider)
    {
        _redisStateService = redisStateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await _db.StreamReadGroupAsync(
                    key: _streamName,
                    groupName: _consumerGroupName,
                    consumerName: _consumerName,
                    position: ">",
                    count: 2,
                    flags: CommandFlags.DemandMaster);

                foreach (var entry in entries)
                {
                    _logger.LogInformation("Processing OrderPendingEvent with ID: {MessageId}", entry.Id);

                    if (entry.Values.Any(x => x.Value == "stream-created"))
                    {
                        await _db.StreamAcknowledgeAsync(_streamName, _consumerGroupName, entry.Id);

                        break;
                    }

                    bool commitMessage = await HandleMessageAsync(entry.Values);

                    if (commitMessage)
                    {
                        await _db.StreamAcknowledgeAsync(_streamName, _consumerGroupName, entry.Id);
                    }
                }

                // Bekleyen mesajları kontrol et (zamanı geldiyse işle)
                await ProcessDueMessagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderPendingEvent: {errorMessage}", ex.Message);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task<bool> HandleMessageAsync(NameValueEntry[] entries)
    {
        bool commitMessage = false;

        var now = DateTime.UtcNow;

        OrderPendingEvent orderPendingEvent = GetEvent(entries);

        Guid idempotencyKey = GetIdempotencyKey(entries);

        EventType eventType = GetEventType(entries);

        // Zaman kontrolü
        if (orderPendingEvent.ScheduledTime <= now)
        {
            // Zamanı gelmiş, hemen işle
            _logger.LogInformation("Processing payment immediately for Order {OrderId}", orderPendingEvent.OrderId);

            await ProcessEvent(orderPendingEvent, idempotencyKey, eventType);

            commitMessage = true;
        }
        else
        {
            // Henüz zamanı gelmedi, beklet
            _logger.LogInformation(
                "Payment scheduled for Order {OrderId}, will process at {ScheduledTime} (in {Minutes} minutes)",
                orderPendingEvent.OrderId,
                orderPendingEvent.ScheduledTime,
                (orderPendingEvent.ScheduledTime - now).TotalMinutes
            );

            // Pending listesine ekle (henüz commit etme!)
            _redisStateService.AddOrderPendingMessage(
                orderPendingEvent.OrderId,
                new OrderPendingRedisHistory(orderPendingEvent, idempotencyKey, eventType));
        }

        return commitMessage;
    }

    private async Task ProcessDueMessagesAsync()
    {
        var duePayments = _redisStateService.PendingMessages
            .Where(kvp => kvp.Value.Event.ScheduledTime <= DateTime.UtcNow)
            .ToList();

        foreach (var (orderId, orderPendingHistory) in duePayments)
        {
            try
            {
                _logger.LogInformation("Processing scheduled payment for Order {OrderId}", orderId);

                await ProcessEvent(orderPendingHistory.Event, orderPendingHistory.IdempotencyKey, orderPendingHistory.EventType);

                // Listeden çıkar
                _redisStateService.RemoveOrderPendingMessage(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for Order {OrderId}", orderId);
            }
        }
    }
}
