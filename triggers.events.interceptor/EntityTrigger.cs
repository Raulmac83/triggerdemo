namespace triggers.events.interceptor;

/// <summary>
/// Base class for a trigger that fires when a single entity type is created, updated, or deleted.
/// Derive and implement <see cref="OnChangedAsync"/>. Register with
/// <c>services.AddEntityTrigger&lt;YourTrigger&gt;()</c>.
/// </summary>
public abstract class EntityTrigger<TEntity> : IEntityChangeHandler<TEntity>
    where TEntity : class
{
    public Task HandleAsync(EntityChange<TEntity> change, CancellationToken ct)
        => OnChangedAsync(change.Type, change.Entity, change.ModifiedProperties, ct);

    protected abstract Task OnChangedAsync(
        EntityChangeType kind,
        TEntity entity,
        IReadOnlyList<PropertyChange> modifiedProperties,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for a trigger that fires when any of the listed entity types is created, updated,
/// or deleted. The handler receives the changed entity as <see cref="object"/> — pattern-match in
/// the override if you need typed access. Extend beyond the supplied generic overloads by
/// deriving from <see cref="MultiEntityTriggerBase"/> directly and overriding
/// <see cref="MultiEntityTriggerBase.WatchedTypes"/>.
/// </summary>
public abstract class MultiEntityTriggerBase : IAnyEntityChangeHandler
{
    protected abstract IReadOnlyList<Type> WatchedTypes { get; }

    Task IAnyEntityChangeHandler.HandleAsync(AnyEntityChange change, CancellationToken ct)
    {
        var watched = WatchedTypes;
        for (var i = 0; i < watched.Count; i++)
        {
            if (watched[i].IsAssignableFrom(change.EntityType))
            {
                return OnChangedAsync(change.Type, change.Entity, change.ModifiedProperties, ct);
            }
        }
        return Task.CompletedTask;
    }

    protected abstract Task OnChangedAsync(
        EntityChangeType kind,
        object entity,
        IReadOnlyList<PropertyChange> modifiedProperties,
        CancellationToken cancellationToken);
}

public abstract class EntityTrigger<T1, T2> : MultiEntityTriggerBase
    where T1 : class
    where T2 : class
{
    protected override IReadOnlyList<Type> WatchedTypes { get; } = new[] { typeof(T1), typeof(T2) };
}

public abstract class EntityTrigger<T1, T2, T3> : MultiEntityTriggerBase
    where T1 : class
    where T2 : class
    where T3 : class
{
    protected override IReadOnlyList<Type> WatchedTypes { get; } = new[] { typeof(T1), typeof(T2), typeof(T3) };
}

public abstract class EntityTrigger<T1, T2, T3, T4> : MultiEntityTriggerBase
    where T1 : class
    where T2 : class
    where T3 : class
    where T4 : class
{
    protected override IReadOnlyList<Type> WatchedTypes { get; } = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
}
