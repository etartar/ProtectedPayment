namespace ProtectedPayment.Database.Abstracts;

public abstract class BaseEntity
{
    private readonly List<IBaseEvent> _events = [];

    protected BaseEntity()
    {
        Id = Guid.CreateVersion7();
    }

    public Guid Id { get; set; }

    public IReadOnlyCollection<IBaseEvent> Events => _events.ToList();

    public void ClearEvents()
    {
        _events.Clear();
    }

    protected void RaiseEvent(IBaseEvent domainEvent)
    {
        _events.Add(domainEvent);
    }
}
