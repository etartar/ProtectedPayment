using ETPackages.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProtectedPayment.Application.Contracts.Orders;
using ProtectedPayment.Presentation.API.Extensions;

namespace ProtectedPayment.Presentation.API.Orders;

public class CancelOrderEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{orderId:guid}/cancel", async (
            Guid orderId,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            var cancelResult = await orderService.CancelOrderAsync(orderId, cancellationToken);

            return cancelResult.Match(Results.NoContent, ApiResults.Problem);
        })
            .WithName("CancelOrder");
    }
}
