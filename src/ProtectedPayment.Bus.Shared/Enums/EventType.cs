namespace ProtectedPayment.Bus.Shared.Enums;

public enum EventType
{
    None = 0,
    OrderPendingEvent = 1,
    OrderConfirmedEvent = 2,
    OrderCancelRequestedEvent = 3,
    OrderCancelledEvent = 4,
    OrderShippedEvent = 5
}