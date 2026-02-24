using ProtectedPayment.Domain.Events;
using ProtectedPayment.Infrastructure.Messaging.Redis;
using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.Worker.Services;
using StackExchange.Redis;

namespace ProtectedPayment.Worker.Consumers.Redis;

internal sealed class OrderPendingEventRedisConsumer : BaseRedisEventConsumer<OrderPendingEvent>
{
    private readonly RedisService _redisStateService;

    public OrderPendingEventRedisConsumer(
        ILogger<BaseRedisEventConsumer<OrderPendingEvent>> logger,
        IRedisConnection redisConnection,
        IServiceProvider serviceProvider,
        RedisService redisStateService) : base(logger, redisConnection, serviceProvider)
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

                    bool commitMessage = await HandleMessageAsync(entry.Values, stoppingToken);

                    if (commitMessage)
                    {
                        await _db.StreamAcknowledgeAsync(_streamName, _consumerGroupName, entry.Id);
                    }
                }

                // Bekleyen mesajları kontrol et (zamanı geldiyse işle)
                await ProcessDueMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderPendingEvent: {errorMessage}", ex.Message);

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task<bool> HandleMessageAsync(NameValueEntry[] entries, CancellationToken cancellationToken)
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

            await ProcessEvent(orderPendingEvent, idempotencyKey, eventType, cancellationToken);

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
            await _redisStateService.AddOrderPendingMessage(
                orderPendingEvent.OrderId,
                orderPendingEvent.ScheduledTime,
                new OrderPendingRedisHistory(orderPendingEvent, idempotencyKey, eventType));
        }

        return commitMessage;
    }

    private async Task ProcessDueMessagesAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var dueOrders = await _redisStateService.GetDueOrders();

        foreach (var orderIdValue in dueOrders)
        {
            Guid orderId = Guid.Parse(orderIdValue.ToString());

            try
            {
                _logger.LogInformation("Processing scheduled payment for Order {OrderId}", orderId);

                OrderPendingRedisHistory? orderPendingHistory = await _redisStateService.GetOrderPendingData(orderId);

                if (orderPendingHistory is null)
                {
                    _logger.LogWarning("No pending data found for Order {OrderId}", orderId);

                    continue;
                }

                await ProcessEvent(orderPendingHistory.Event, orderPendingHistory.IdempotencyKey, orderPendingHistory.EventType, cancellationToken);

                // Listeden çıkar
                await _redisStateService.RemoveOrderPendingMessage(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for Order {OrderId}", orderId);
            }
        }
    }
}
