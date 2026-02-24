using Newtonsoft.Json;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.SharedKernel.Serialization;

namespace ProtectedPayment.Worker.Inboxes;

internal sealed class OrderPendingEventInboxConsumer(ILogger<OrderPendingEventInboxConsumer> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var inboxMessageRepository = scope.ServiceProvider.GetRequiredService<IInboxMessageRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            List<InboxMessage> inboxMessages = await inboxMessageRepository.GetUnprocessedMessagesAsync(EventType.OrderPendingEvent, stoppingToken);

            foreach (var inboxMessage in inboxMessages)
            {
                var orderPendingEvent = JsonConvert.DeserializeObject<OrderPendingEvent>(inboxMessage.Content, SerializerSettings.Instance);

                logger.LogInformation($"Processing OrderPendingEvent for OrderId: {orderPendingEvent!.OrderId}");

                try
                {
                    var getOrder = await orderRepository.GetOrderAsync(orderPendingEvent.OrderId, stoppingToken);

                    if (getOrder != null)
                    {
                        getOrder.Confirm();

                        await orderRepository.UpdateAsync(getOrder);

                        logger.LogInformation($"Order with OrderId: {orderPendingEvent.OrderId} has been confirmed.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing OrderPendingEvent for OrderId: {orderPendingEvent.OrderId}");

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
