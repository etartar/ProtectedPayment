using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.Worker.Consumers.Kafka;
using ProtectedPayment.Worker.Consumers.RabbitMQ;
using ProtectedPayment.Worker.Consumers.Redis;
using ProtectedPayment.Worker.Inboxes;
using ProtectedPayment.Worker.Services;

namespace ProtectedPayment.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsumers(this IServiceCollection services)
    {
        var serviceTypeStr = Environment.GetEnvironmentVariable("BUS_SERVICE_TYPE");

        if (!Enum.TryParse<BusServiceType>(serviceTypeStr, out var busServiceType))
        {
            throw new InvalidOperationException($"Invalid BusServiceType: {serviceTypeStr}");
        }

        services.AddRabbitMQConsumers(busServiceType);
        services.AddKafkaConsumers(busServiceType);
        services.AddRedisConsumers(busServiceType);

        services.AddHostedService<OrderPendingEventInboxConsumer>();
        services.AddHostedService<OrderShippedEventInboxConsumer>();
        services.AddHostedService<OrderCancelRequestedEventInboxConsumer>();

        return services;
    }

    private static void AddRabbitMQConsumers(this IServiceCollection services, BusServiceType busServiceType)
    {
        if (busServiceType == BusServiceType.RabbitMQ)
        {
            services.AddHostedService<OrderPendingEventConsumer>();
            services.AddHostedService<OrderShippedEventConsumer>();
            services.AddHostedService<OrderCancelRequestedEventConsumer>();
        }
    }

    private static void AddKafkaConsumers(this IServiceCollection services, BusServiceType busServiceType)
    {
        if (busServiceType == BusServiceType.Kafka)
        {
            services.AddSingleton<KafkaService>();

            services.AddHostedService<OrderPendingEventKafkaConsumer>();
            services.AddHostedService<OrderShippedEventKafkaConsumer>();
            services.AddHostedService<OrderCancelRequestedEventKafkaConsumer>();
        }
    }

    private static void AddRedisConsumers(this IServiceCollection services, BusServiceType busServiceType)
    {
        if (busServiceType == BusServiceType.Redis)
        {
            services.AddSingleton<RedisService>();

            services.AddHostedService<OrderPendingEventRedisConsumer>();
            services.AddHostedService<OrderShippedEventRedisConsumer>();
            services.AddHostedService<OrderCancelRequestedEventRedisConsumer>();
        }
    }
}
