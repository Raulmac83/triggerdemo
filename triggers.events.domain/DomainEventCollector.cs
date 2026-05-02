namespace triggers.events.domain;

/// <summary>
/// Optional helper for entities that don't want to wire up the boilerplate themselves.
/// Compose into your entity (or copy the pattern).
/// </summary>
public sealed class DomainEventCollector
{
    private readonly List<IDomainEvent> _events = new();

    public IReadOnlyCollection<IDomainEvent> Events => _events;

    public void Raise(IDomainEvent e) => _events.Add(e);

    public void Clear() => _events.Clear();
}
