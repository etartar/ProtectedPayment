using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.Infrastructure.Database;
using ProtectedPayment.Infrastructure.Database.Interceptors;
using ProtectedPayment.Infrastructure.Messaging.Kafka;
using ProtectedPayment.Infrastructure.Messaging.RabbitMQ;
using ProtectedPayment.Infrastructure.Messaging.Redis;
using ProtectedPayment.Infrastructure.Repositories;
using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceTypeStr = Environment.GetEnvironmentVariable("BUS_SERVICE_TYPE");

        if (!Enum.TryParse<BusServiceType>(serviceTypeStr, out var busServiceType))
        {
            throw new InvalidOperationException($"Invalid BusServiceType: {serviceTypeStr}");
        }

        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        services.AddDbContext<OrderDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        services.TryAddScoped<IOrderRepository, OrderRepository>();

        services.TryAddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        services.TryAddScoped<IInboxMessageRepository, InboxMessageRepository>();

        services.TryAddScoped<IIdempotencyKeyRepository, IdempotencyKeyRepository>();

        services.AddRabbitMQImplementation(configuration, busServiceType);

        services.AddKafkaImplementation(configuration, busServiceType);

        services.AddRedisImplementation(configuration, busServiceType);

        return services;
    }

    private static void AddRabbitMQImplementation(
        this IServiceCollection services,
        IConfiguration configuration,
        BusServiceType busServiceType)
    {
        if (busServiceType == BusServiceType.RabbitMQ)
        {
            services.AddRabbitMQ(configuration, createExchanges: true);
        }
    }

    private static void AddKafkaImplementation(
        this IServiceCollection services,
        IConfiguration configuration,
        BusServiceType busServiceType)
    {
        if (busServiceType == BusServiceType.Kafka)
        {
            services.AddKafka(configuration, createTopics: true);
        }
    }

    private static void AddRedisImplementation(
        this IServiceCollection services,
        IConfiguration configuration,
        BusServiceType busServiceType)
    {
        if (busServiceType == BusServiceType.Redis)
        {
            services.AddRedis(configuration, createStreams: true);
        }
    }
}
