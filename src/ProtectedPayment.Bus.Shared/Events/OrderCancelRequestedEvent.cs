namespace ProtectedPayment.Bus.Shared.Events;

public record OrderCancelRequestedEvent(Guid OrderId) : BaseEvent;
