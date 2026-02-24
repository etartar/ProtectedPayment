using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Application.Contracts.Orders;

public record GetOrdersDto(
    Guid Id,
    string OrderNumber,
    string ProductName,
    decimal Amount,
    OrderStatus Status,
    DateTime CreatedAt);
