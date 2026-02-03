using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Serialization;
using ProtectedPayment.Database.Database;
using ProtectedPayment.Database.Entities;

namespace ProtectedPayment.Worker.Inboxes;

internal sealed class OrderPendingEventInboxConsumer(ILogger<OrderPendingEventInboxConsumer> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            List<InboxMessage> inboxMessages = dbContext
                .InboxMessages
                .Where(x => x.Type == $"{EventType.OrderPendingEvent}" && x.ProcessedOnUtc == null)
                .Take(100)
                .ToList();

            foreach (var inboxMessage in inboxMessages)
            {
                var orderPendingEvent = JsonConvert.DeserializeObject<OrderPendingEvent>(inboxMessage.Content, SerializerSettings.Instance);

                logger.LogInformation($"Processing OrderPendingEvent for OrderId: {orderPendingEvent!.OrderId}");

                try
                {
                    var getOrder = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderPendingEvent.OrderId, stoppingToken);

                    if (getOrder != null)
                    {
                        getOrder.Confirm();

                        dbContext.Orders.Update(getOrder);

                        logger.LogInformation($"Order with OrderId: {orderPendingEvent.OrderId} has been confirmed.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing OrderPendingEvent for OrderId: {orderPendingEvent.OrderId}");

                    inboxMessage.Error = ex.Message;
                }

                inboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
