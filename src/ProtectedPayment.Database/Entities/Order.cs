using ProtectedPayment.Database.Abstracts;

namespace ProtectedPayment.Database.Entities;

public class Order : BaseEntity
{
    private Order(string productName, decimal amount, OrderStatus status) : base()
    {
        OrderNumber = $"ORD-{Guid.CreateVersion7().ToString("N").ToUpperInvariant()}";
        ProductName = productName;
        Amount = amount;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        PendingUntil = CreatedAt.AddMinutes(10);
    }

    public string OrderNumber { get; set; }
    public string ProductName { get; set; }
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PendingUntil { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public static Order Create(string productName, decimal amount)
    {
        var createOrder = new Order(productName, amount, OrderStatus.Pending);

        //createOrder.RaiseEvent();

        return createOrder;
    }
}
