namespace ProtectedPayment.Application.Contracts.OutboxMessages;

public interface IOutboxMessageService
{
    Task ProcessUnprocessedMessages(CancellationToken cancellationToken);
}
