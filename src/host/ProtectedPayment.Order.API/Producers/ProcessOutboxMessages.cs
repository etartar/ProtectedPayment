using ProtectedPayment.Application.Contracts.OutboxMessages;

namespace ProtectedPayment.Order.API.Producers;

public class ProcessOutboxMessages(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var outboxMessageService = scope.ServiceProvider.GetRequiredService<IOutboxMessageService>();
            
            await outboxMessageService.ProcessUnprocessedMessages(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
