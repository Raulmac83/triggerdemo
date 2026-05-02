using triggers.events.domain;

namespace triggers.db.Entities;

public partial class Trigger : IEventfulEntity
{
    private readonly DomainEventCollector _events = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.Events;

    public void ClearDomainEvents() => _events.Clear();

    public void RaiseDomainEvent(IDomainEvent e) => _events.Raise(e);
}
