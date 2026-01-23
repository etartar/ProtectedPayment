using ETPackages.Endpoints;
using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Database.Database;
using ProtectedPayment.Database.Entities;

namespace ProtectedPayment.Order.API.Endpoints;

internal sealed class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (OrderDbContext dbContext, CancellationToken cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var getOrders = await dbContext
                .Orders
                .AsNoTracking()
                .Select(o => new GetOrdersDto(
                    o.Id,
                    o.OrderNumber,
                    o.ProductName,
                    o.Amount,
                    o.Status,
                    o.CreatedAt))
                .ToListAsync(cancellationToken);

            return getOrders;
        })
            .WithName("GetOrders");
    }

    public record GetOrdersDto(
        Guid Id,
        string OrderNumber,
        string ProductName,
        decimal Amount,
        OrderStatus Status,
        DateTime CreatedAt);
}
