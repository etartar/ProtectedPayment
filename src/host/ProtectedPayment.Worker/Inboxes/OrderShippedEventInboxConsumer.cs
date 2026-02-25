using Newtonsoft.Json;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.SharedKernel.Serialization;

namespace ProtectedPayment.Worker.Inboxes;

internal sealed class OrderShippedEventInboxConsumer(ILogger<OrderShippedEventInboxConsumer> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var inboxMessageRepository = scope.ServiceProvider.GetRequiredService<IInboxMessageRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            List<InboxMessage> inboxMessages = await inboxMessageRepository.GetUnprocessedMessagesAsync(EventType.OrderShippedEvent, stoppingToken);

            foreach (var inboxMessage in inboxMessages)
            {
                var orderShippedEvent = JsonConvert.DeserializeObject<OrderShippedEvent>(inboxMessage.Content, SerializerSettings.Instance);

                logger.LogInformation($"Processing OrderShippedEvent for OrderId: {orderShippedEvent!.OrderId}");

                try
                {
                    var getOrder = await orderRepository.GetOrderAsync(orderShippedEvent.OrderId, stoppingToken);

                    if (getOrder != null)
                    {
                        getOrder.Ship();

                        await orderRepository.UpdateAsync(getOrder);

                        logger.LogInformation($"Order with OrderId: {orderShippedEvent.OrderId} has been shipped.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing OrderShippedEvent for OrderId: {orderShippedEvent.OrderId}");

                    inboxMessage.Error = ex.Message;
                }

                inboxMessage.ProcessedOnUtc = DateTime.UtcNow;

                await inboxMessageRepository.UpdateAsync(inboxMessage);
            }

            await unitOfWork.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
