using Confluent.Kafka;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Worker.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProtectedPayment.Worker.Consumers.Redis;

internal sealed class OrderCancelRequestedEventRedisConsumer : BaseRedisEventConsumer<OrderCancelRequestedEvent>
{
    private readonly RedisStateService _redisStateService;

    public OrderCancelRequestedEventRedisConsumer(
        ILogger<BaseRedisEventConsumer<OrderCancelRequestedEvent>> logger,
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
                    _logger.LogInformation("Processing OrderCancelRequestedEvent with ID: {MessageId}", entry.Id);

                    if (entry.Values.Any(x => x.Value == "stream-created"))
                    {
                        await _db.StreamAcknowledgeAsync(_streamName, _consumerGroupName, entry.Id);

                        break;
                    }

                    Guid idempotencyKey = GetIdempotencyKey(entry.Values);

                    EventType eventType = GetEventType(entry.Values);

                    var orderCancelRequestedEvent = GetEvent(entry.Values);

                    _logger.LogInformation("Cancelling payment for Order {OrderId}", orderCancelRequestedEvent!.OrderId);

                    // Pending listesinden çıkar
                    _redisStateService.RemoveOrderPendingMessage(orderCancelRequestedEvent!.OrderId);

                    await ProcessEvent(orderCancelRequestedEvent, idempotencyKey, eventType);

                    await _db.StreamAcknowledgeAsync(_streamName, _consumerGroupName, entry.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderCancelRequestedEvent: {errorMessage}", ex.Message);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
