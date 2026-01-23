using ETPackages.Endpoints;
using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Database.Database;

namespace ProtectedPayment.Order.API.Endpoints;

public class CancelOrderEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{orderId:guid}/cancel", async (
            Guid orderId,
            OrderDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var getOrder = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

            if (getOrder is null)
            {
                return Results.NotFound("Sipariş bulunamadı.");
            }

            getOrder.RequestCancellation();

            dbContext.Orders.Update(getOrder);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created();
        })
            .WithName("CancelOrder");
    }

    public record CreateOrderRequest(string ProductName, decimal Amount);
}
