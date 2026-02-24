namespace ProtectedPayment.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
