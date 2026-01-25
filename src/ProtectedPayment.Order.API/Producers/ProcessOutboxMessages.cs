using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Constants;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Serialization;
using ProtectedPayment.Database.Database;

namespace ProtectedPayment.Order.API.Producers;

public class ProcessOutboxMessages(
    ILogger<ProcessOutboxMessages> logger,
    IServiceProvider serviceProvider,
    IBusService busService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            
            var outboxMessages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null)
                .ToListAsync(stoppingToken);

            foreach (var message in outboxMessages)
            {
                try
                {
                    var headers = new Dictionary<string, object?>
                    {
                        { QueueConstants.IdempotencyKey, message.IdempotencyKey.ToString() },
                        { QueueConstants.EventType, message.Type }
                    };

                    var getEvent = JsonConvert.DeserializeObject<BaseEvent>(message.Content, SerializerSettings.Instance);

                    await Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(
                            retryCount: 3,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            onRetry: (ex, ts, retryCount, ctx) =>
                            {
                                logger.LogWarning(
                                    ex,
                                    "Retry {RetryCount} for outbox message {MessageId} after {TimeSpan}",
                                    retryCount,
                                    message.Id,
                                    ts);
                            })
                        .ExecuteAsync(async () => await busService.PublishAsync(getEvent!, headers!));
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Exception while processing outbox message {MessageId}",
                        message.Id);

                    message.Error = ex?.ToString();
                }

                message.ProcessedOnUtc = DateTime.UtcNow;

                dbContext.OutboxMessages.Update(message);
            }

            await dbContext.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
