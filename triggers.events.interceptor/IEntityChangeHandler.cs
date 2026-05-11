namespace triggers.events.interceptor;

public interface IEntityChangeHandler<TEntity> where TEntity : class
{
    Task HandleAsync(EntityChange<TEntity> change, CancellationToken ct);
}

/// <summary>
/// A handler that receives change events for any tracked entity type. Used by multi-entity
/// triggers; the implementation is responsible for filtering by entity type.
/// </summary>
public interface IAnyEntityChangeHandler
{
    Task HandleAsync(AnyEntityChange change, CancellationToken ct);
}

public sealed record AnyEntityChange(
    EntityChangeType Type,
    Type EntityType,
    object Entity,
    IReadOnlyList<PropertyChange> ModifiedProperties);
