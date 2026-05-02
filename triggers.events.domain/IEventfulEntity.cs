namespace triggers.events.domain;

/// <summary>
/// Implement on entities that raise domain events. The dispatching interceptor reads and clears them after a successful save.
/// </summary>
public interface IEventfulEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
