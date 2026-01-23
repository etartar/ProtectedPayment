using ETPackages.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Database.Database;

namespace ProtectedPayment.Order.API.Endpoints;

public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            [FromBody] CreateOrderRequest request,
            OrderDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var createOrder = Database.Entities.Order.Create(request.ProductName, request.Amount);

            await dbContext.Orders.AddAsync(createOrder, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new
            {
                OrderId = createOrder.Id,
                Message = "Order created successfully"
            });
        })
            .WithName("CreateOrder");
    }

    public record CreateOrderRequest(string ProductName, decimal Amount);
}
