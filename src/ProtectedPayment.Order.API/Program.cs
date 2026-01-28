using ETPackages.Endpoints;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Extensions;
using ProtectedPayment.Database;
using ProtectedPayment.Order.API.Extensions;
using ProtectedPayment.Order.API.Producers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDatabaseProvider(builder.Configuration);

builder.Services.AddEndpoints(typeof(Program).Assembly);

var busServiceType = builder.Configuration.GetValue<BusServiceType>("ServiceBusOption:BusServiceType");

if (busServiceType == BusServiceType.RabbitMQ)
{
    builder.Services.AddRabbitMQ(builder.Configuration);
}

if (busServiceType == BusServiceType.Kafka)
{
    builder.Services.AddKafka(builder.Configuration);
}

builder.Services.AddHostedService<ProcessOutboxMessages>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
