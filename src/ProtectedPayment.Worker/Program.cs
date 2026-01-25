using ProtectedPayment.Bus.Shared.Extensions;
using ProtectedPayment.Database;
using ProtectedPayment.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDatabaseProvider(builder.Configuration);

builder.Services.AddRabbitMQ(builder.Configuration);

builder.Services.AddHostedService<OrderPendingEventConsumer>();
builder.Services.AddHostedService<OrderPendingEventInboxConsumer>();

builder.Services.AddHostedService<OrderShippedEventConsumer>();
builder.Services.AddHostedService<OrderShippedEventInboxConsumer>();

builder.Services.AddHostedService<OrderCancelRequestedEventConsumer>();
builder.Services.AddHostedService<OrderCancelRequestedEventInboxConsumer>();

var host = builder.Build();
host.Run();
