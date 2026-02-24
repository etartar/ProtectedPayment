using ETPackages.Result;

namespace ProtectedPayment.Application.Contracts.Orders;

public interface IOrderService
{
    Task<List<GetOrdersDto>> GetOrdersAsync(CancellationToken cancellationToken);
    Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<Result> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken);
}
