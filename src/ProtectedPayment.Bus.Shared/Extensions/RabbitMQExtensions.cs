using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Options;
using ProtectedPayment.Bus.Shared.Services;

namespace ProtectedPayment.Bus.Shared.Extensions;

public static class RabbitMQExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceBusOption>(configuration.GetSection(nameof(ServiceBusOption)));

        services.AddSingleton<ServiceBusOption>(sp =>
        {
            var optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();

            return optionsServiceBus.Value;
        });

        services.AddSingleton<IBusService, RabbitMQBusService>(sp =>
        {
            var serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();

            var rabbitMqBus = new RabbitMQBusService(serviceBusOptions);
            rabbitMqBus.Init().Wait();
            rabbitMqBus.CreateExchanges().Wait();

            return rabbitMqBus;
        });

        return services;
    }
}
