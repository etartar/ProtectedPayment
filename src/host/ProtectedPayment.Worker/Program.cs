using ProtectedPayment.Application;
using ProtectedPayment.Infrastructure;
using ProtectedPayment.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddConsumers();

var host = builder.Build();
host.Run();
