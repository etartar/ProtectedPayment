using ETPackages.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProtectedPayment.Application.Contracts.Orders;

namespace ProtectedPayment.Presentation.API.Orders;

internal sealed class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            var getOrders = await orderService.GetOrdersAsync(cancellationToken);

            return getOrders;
        })
            .WithName("GetOrders");
    }
}
