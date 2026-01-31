using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Extensions;
using ProtectedPayment.Database;
using ProtectedPayment.Worker.Consumers.Kafka;
using ProtectedPayment.Worker.Consumers.RabbitMQ;
using ProtectedPayment.Worker.Consumers.Redis;
using ProtectedPayment.Worker.Inboxes;
using ProtectedPayment.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDatabaseProvider(builder.Configuration);

var serviceTypeStr = Environment.GetEnvironmentVariable("BUS_SERVICE_TYPE");

if (!Enum.TryParse<BusServiceType>(serviceTypeStr, out var busServiceType))
{
    throw new InvalidOperationException($"Invalid BusServiceType: {serviceTypeStr}");
}

if (busServiceType == BusServiceType.RabbitMQ)
{
    builder.Services.AddRabbitMQ(builder.Configuration, createExchanges: true);

    builder.Services.AddHostedService<OrderPendingEventConsumer>();
    builder.Services.AddHostedService<OrderShippedEventConsumer>();
    builder.Services.AddHostedService<OrderCancelRequestedEventConsumer>();
}
else if (busServiceType == BusServiceType.Kafka)
{
    builder.Services.AddSingleton<KafkaStateService>();
    builder.Services.AddKafka(builder.Configuration, createTopics: true);

    builder.Services.AddHostedService<OrderPendingEventKafkaConsumer>();
    builder.Services.AddHostedService<OrderShippedEventKafkaConsumer>();
    builder.Services.AddHostedService<OrderCancelRequestedEventKafkaConsumer>();
}
else if (busServiceType == BusServiceType.Redis)
{
    builder.Services.AddSingleton<RedisStateService>();
    builder.Services.AddRedis(builder.Configuration, createStreams: true);

    builder.Services.AddHostedService<OrderPendingEventRedisConsumer>();
    builder.Services.AddHostedService<OrderShippedEventRedisConsumer>();
    builder.Services.AddHostedService<OrderCancelRequestedEventRedisConsumer>();
}

builder.Services.AddHostedService<OrderPendingEventInboxConsumer>();
builder.Services.AddHostedService<OrderShippedEventInboxConsumer>();
builder.Services.AddHostedService<OrderCancelRequestedEventInboxConsumer>();

var host = builder.Build();
host.Run();
