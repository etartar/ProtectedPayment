using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Options;
using ProtectedPayment.Bus.Shared.Services;

namespace ProtectedPayment.Bus.Shared.Extensions;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration, bool createTopics = false)
    {
        services.Configure<ServiceBusOption>(configuration.GetSection(nameof(ServiceBusOption)));

        services.AddSingleton<ServiceBusOption>(sp =>
        {
            var optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();

            return optionsServiceBus.Value;
        });

        services.AddSingleton<IBusService, KafkaBusService>(sp =>
        {
            var serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();

            var logger = sp.GetRequiredService<ILogger<KafkaBusService>>();

            var kafkaBus = new KafkaBusService(logger, serviceBusOptions);
            
            if (createTopics)
            {
                kafkaBus.CreateTopics().Wait();
            }

            return kafkaBus;
        });

        return services;
    }
}
