namespace ProtectedPayment.Bus.Shared.Events;

public record OrderShippedEvent(Guid OrderId) : BaseEvent(OrderId);
