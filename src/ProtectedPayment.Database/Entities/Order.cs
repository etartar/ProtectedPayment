using ProtectedPayment.Bus.Shared.Events;
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

        createOrder.RaiseEvent(new OrderPendingEvent(createOrder.Id));

        return createOrder;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be confirmed.");
        }

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        RaiseEvent(new OrderConfirmedEvent(Id));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed orders can be shipped.");
        }

        Status = OrderStatus.Shipped;

        RaiseEvent(new OrderShippedEvent(Id));
    }

    public void RequestCancellation()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can request cancellation.");
        }

        Status = OrderStatus.CancelRequested;

        RaiseEvent(new OrderCancelRequestedEvent(Id));
    }

    public void ConfirmCancellation()
    {
        if (Status != OrderStatus.CancelRequested)
        {
            throw new InvalidOperationException("Only cancel requested orders can be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;

        RaiseEvent(new OrderCancelledEvent(Id));
    }
}
