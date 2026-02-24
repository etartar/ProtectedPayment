using Newtonsoft.Json;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.SharedKernel.Serialization;

namespace ProtectedPayment.Worker.Inboxes;

internal sealed class OrderCancelRequestedEventInboxConsumer(ILogger<OrderCancelRequestedEventInboxConsumer> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var inboxMessageRepository = scope.ServiceProvider.GetRequiredService<IInboxMessageRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            List<InboxMessage> inboxMessages = await inboxMessageRepository.GetUnprocessedMessagesAsync(EventType.OrderCancelRequestedEvent, stoppingToken);

            foreach (var inboxMessage in inboxMessages)
            {
                var orderCancelRequestedEvent = JsonConvert.DeserializeObject<OrderCancelRequestedEvent>(inboxMessage.Content, SerializerSettings.Instance);

                logger.LogInformation($"Processing OrderCancelRequestedEvent for OrderId: {orderCancelRequestedEvent!.OrderId}");

                try
                {
                    var getOrder = await orderRepository.GetOrderAsync(orderCancelRequestedEvent.OrderId, stoppingToken);

                    if (getOrder != null)
                    {
                        getOrder.ConfirmCancellation();

                        await orderRepository.UpdateAsync(getOrder);

                        logger.LogInformation($"Order with OrderId: {orderCancelRequestedEvent.OrderId} has been cancelled.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing OrderCancelRequestedEvent for OrderId: {orderCancelRequestedEvent.OrderId}");

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
