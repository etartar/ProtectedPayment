using ETPackages.Endpoints;
using ProtectedPayment.Application;
using ProtectedPayment.Infrastructure;
using ProtectedPayment.Order.API.Extensions;
using ProtectedPayment.Order.API.Producers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(ProtectedPayment.Presentation.API.AssemblyReference.Assembly);

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