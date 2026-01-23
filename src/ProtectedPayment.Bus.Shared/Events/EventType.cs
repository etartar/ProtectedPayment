namespace ProtectedPayment.Bus.Shared.Events;

public enum EventType
{
    OrderPendingEvent = 1,
    OrderConfirmedEvent = 2,
    OrderCancelRequestedEvent = 3,
    OrderCancelledEvent = 4,
    OrderShippedEvent = 5
}