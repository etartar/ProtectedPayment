using ETPackages.Result;
using ProtectedPayment.Application.Contracts.Orders;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Errors;
using ProtectedPayment.Domain.Repositories;

namespace ProtectedPayment.Application.Orders;

internal sealed class OrderService(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<List<GetOrdersDto>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOrdersAsync(cancellationToken);

        return orders.Select(order => new GetOrdersDto(
            Id: order.Id,
            OrderNumber: order.OrderNumber,
            ProductName: order.ProductName,
            Amount: order.Amount,
            Status: order.Status,
            CreatedAt: order.CreatedAt))
            .ToList();
    }

    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var createOrder = Order.Create(request.ProductName, request.Amount);

        await orderRepository.AddAsync(createOrder, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateOrderResponse
        (
            OrderId: createOrder.Id,
            Message: "Order created successfully."
        );

        return response;
    }

    public async Task<Result> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var getOrder = await orderRepository.GetOrderAsync(orderId, cancellationToken);

        if (getOrder is null)
        {
            return Result.Failure(OrderErrors.NotFound(orderId));
        }

        getOrder.RequestCancellation();

        await orderRepository.UpdateAsync(getOrder);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
