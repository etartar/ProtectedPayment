using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using ProtectedPayment.Application.Contracts.Contracts;
using ProtectedPayment.Application.Contracts.OutboxMessages;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.SharedKernel.Constants;
using ProtectedPayment.SharedKernel.Events;
using ProtectedPayment.SharedKernel.Serialization;

namespace ProtectedPayment.Application.OutboxMessages;

internal sealed class OutboxMessageService(
    ILogger<OutboxMessageService> logger,
    IBusService busService,
    IOutboxMessageRepository outboxMessageRepository,
    IUnitOfWork unitOfWork) : IOutboxMessageService
{
    public async Task ProcessUnprocessedMessages(CancellationToken cancellationToken)
    {
        var outboxMessages = await outboxMessageRepository.GetUnprocessedMessagesAsync(cancellationToken);

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

                headers.Add(QueueConstants.MessageKey, getEvent!.MessageKey.ToString());

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

            await outboxMessageRepository.UpdateAsync(message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
