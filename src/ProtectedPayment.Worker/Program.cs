using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Extensions;
using ProtectedPayment.Database;
using ProtectedPayment.Worker.Consumers.Kafka;
using ProtectedPayment.Worker.Consumers.RabbitMQ;
using ProtectedPayment.Worker.Inboxes;
using ProtectedPayment.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDatabaseProvider(builder.Configuration);

builder.Services.AddSingleton<KafkaStateService>();

var busServiceType = builder.Configuration.GetValue<BusServiceType>("ServiceBusOption:BusServiceType");

if (busServiceType == BusServiceType.RabbitMQ)
{
    builder.Services.AddRabbitMQ(builder.Configuration);

    builder.Services.AddHostedService<OrderPendingEventConsumer>();
    builder.Services.AddHostedService<OrderShippedEventConsumer>();
    builder.Services.AddHostedService<OrderCancelRequestedEventConsumer>();
}

if (busServiceType == BusServiceType.Kafka)
{
    builder.Services.AddKafka(builder.Configuration);

    builder.Services.AddHostedService<OrderPendingEventKafkaConsumer>();
    builder.Services.AddHostedService<OrderShippedEventKafkaConsumer>();
    builder.Services.AddHostedService<OrderCancelRequestedEventKafkaConsumer>();
}

builder.Services.AddHostedService<OrderPendingEventInboxConsumer>();
builder.Services.AddHostedService<OrderShippedEventInboxConsumer>();
builder.Services.AddHostedService<OrderCancelRequestedEventInboxConsumer>();

var host = builder.Build();
host.Run();
