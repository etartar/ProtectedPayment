using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Serialization;
using ProtectedPayment.Database.Database;
using ProtectedPayment.Database.Entities;

namespace ProtectedPayment.Worker.Inboxes;

internal sealed class OrderShippedEventInboxConsumer(ILogger<OrderShippedEventInboxConsumer> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            List<InboxMessage> inboxMessages = dbContext
                .InboxMessages
                .Where(x => x.Type == $"{EventType.OrderShippedEvent}" && x.ProcessedOnUtc == null)
                .Take(100)
                .ToList();

            foreach (var inboxMessage in inboxMessages)
            {
                var orderShippedEvent = JsonConvert.DeserializeObject<OrderShippedEvent>(inboxMessage.Content, SerializerSettings.Instance);

                logger.LogInformation($"Processing OrderShippedEvent for OrderId: {orderShippedEvent!.OrderId}");

                try
                {
                    var getOrder = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderShippedEvent.OrderId, stoppingToken);

                    if (getOrder != null)
                    {
                        getOrder.Ship();

                        dbContext.Orders.Update(getOrder);

                        logger.LogInformation($"Order with OrderId: {orderShippedEvent.OrderId} has been shipped.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing OrderShippedEvent for OrderId: {orderShippedEvent.OrderId}");

                    inboxMessage.Error = ex.Message;
                }

                inboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
