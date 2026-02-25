namespace ProtectedPayment.Application.Contracts.Orders;

public record CreateOrderRequest(string ProductName, decimal Amount);
