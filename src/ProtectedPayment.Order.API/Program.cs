using ETPackages.Endpoints;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Extensions;
using ProtectedPayment.Database;
using ProtectedPayment.Order.API.Extensions;
using ProtectedPayment.Order.API.Producers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDatabaseProvider(builder.Configuration);

builder.Services.AddEndpoints(typeof(Program).Assembly);

var serviceTypeStr = Environment.GetEnvironmentVariable("BUS_SERVICE_TYPE");

if (!Enum.TryParse<BusServiceType>(serviceTypeStr, out var busServiceType))
{
    throw new InvalidOperationException($"Invalid BusServiceType: {serviceTypeStr}");
}

if (busServiceType == BusServiceType.RabbitMQ)
{
    builder.Services.AddRabbitMQ(builder.Configuration, createExchanges: true);
}
else if (busServiceType == BusServiceType.Kafka)
{
    builder.Services.AddKafka(builder.Configuration, createTopics: true);
}
else if (busServiceType == BusServiceType.Redis)
{
    builder.Services.AddRedis(builder.Configuration, createStreams: true);
}

builder.Services.AddHostedService<ProcessOutboxMessages>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
