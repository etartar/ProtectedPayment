using ETPackages.Endpoints;
using ProtectedPayment.Database;
using ProtectedPayment.Order.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDatabaseProvider(builder.Configuration);

builder.Services.AddEndpoints(typeof(Program).Assembly);

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
