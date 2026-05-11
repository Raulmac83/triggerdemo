using EntityFrameworkCore.Triggered;

namespace triggers.events.efcoretriggered;

public enum EntityChangeKind
{
    Added,
    Modified,
    Deleted,
}

internal static class EntityChangeKindMap
{
    public static EntityChangeKind? From(ChangeType type) => type switch
    {
        ChangeType.Added    => EntityChangeKind.Added,
        ChangeType.Modified => EntityChangeKind.Modified,
        ChangeType.Deleted  => EntityChangeKind.Deleted,
        _ => null,
    };
}

/// <summary>
/// Base class for a trigger that fires when a single entity type is created, updated, or deleted.
/// Derive and implement <see cref="OnChangedAsync"/>.
/// </summary>
public abstract class EntityTrigger<TEntity> : IAfterSaveTrigger<TEntity>
    where TEntity : class
{
    public Task AfterSave(ITriggerContext<TEntity> context, CancellationToken cancellationToken)
    {
        var kind = EntityChangeKindMap.From(context.ChangeType);
        if (kind is null) return Task.CompletedTask;
        return OnChangedAsync(kind.Value, context.Entity, context.UnmodifiedEntity, cancellationToken);
    }

    protected abstract Task OnChangedAsync(
        EntityChangeKind kind,
        TEntity entity,
        TEntity? unmodifiedEntity,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for a trigger that fires when any of the listed entity types is created, updated, or
/// deleted. Override <see cref="WatchedTypes"/> to extend beyond the supplied generic overloads.
/// </summary>
public abstract class MultiEntityTriggerBase : IAfterSaveTrigger<object>
{
    protected abstract IReadOnlyList<Type> WatchedTypes { get; }

    public Task AfterSave(ITriggerContext<object> context, CancellationToken cancellationToken)
    {
        var entity = context.Entity;
        if (entity is null) return Task.CompletedTask;

        var entityType = entity.GetType();
        var matched = false;
        for (var i = 0; i < WatchedTypes.Count; i++)
        {
            if (WatchedTypes[i].IsAssignableFrom(entityType)) { matched = true; break; }
        }
        if (!matched) return Task.CompletedTask;

        var kind = EntityChangeKindMap.From(context.ChangeType);
        if (kind is null) return Task.CompletedTask;

        return OnChangedAsync(kind.Value, entity, context.UnmodifiedEntity, cancellationToken);
    }

    protected abstract Task OnChangedAsync(
        EntityChangeKind kind,
        object entity,
        object? unmodifiedEntity,
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
