using ETPackages.Endpoints;
using ETPackages.Result;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProtectedPayment.Application.Contracts.Orders;
using ProtectedPayment.Presentation.API.Extensions;

namespace ProtectedPayment.Presentation.API.Orders;

public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            [FromBody] CreateOrderRequest request,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            Result<CreateOrderResponse> result = await orderService.CreateOrderAsync(request, cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
            .WithName("CreateOrder");
    }
}
