using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProtectedPayment.Application.Contracts.Contracts;
using ProtectedPayment.SharedKernel.Options;

namespace ProtectedPayment.Infrastructure.Messaging.RabbitMQ;

public static class RabbitMQExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration, bool createExchanges = false)
    {
        services.Configure<ServiceBusOption>(configuration.GetSection(nameof(ServiceBusOption)));

        services.AddSingleton<ServiceBusOption>(sp =>
        {
            var optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();

            return optionsServiceBus.Value;
        });

        services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>(sp =>
        {
            var serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();

            var rabbitMqBus = new RabbitMQConnection(serviceBusOptions);

            rabbitMqBus.Init().Wait();

            return rabbitMqBus;
        });

        services.AddSingleton<IBusService, RabbitMQBusService>(sp =>
        {
            var rabbitMQService = sp.GetRequiredService<IRabbitMQConnection>();

            var rabbitMqBus = new RabbitMQBusService(rabbitMQService);
            
            if (createExchanges)
            {
                rabbitMqBus.CreateExchanges().Wait();
            }

            return rabbitMqBus;
        });

        return services;
    }
}
