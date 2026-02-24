namespace ProtectedPayment.Application.Contracts.Orders;

public record CreateOrderResponse(Guid OrderId, string Message);