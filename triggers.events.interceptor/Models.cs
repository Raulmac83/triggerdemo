namespace triggers.events.interceptor;

public enum EntityChangeType
{
    Added,
    Modified,
    Deleted,
}

public sealed record PropertyChange(string Name, object? OldValue, object? NewValue);

public sealed record EntityChange<TEntity>(
    EntityChangeType Type,
    TEntity Entity,
    IReadOnlyList<PropertyChange> ModifiedProperties)
    where TEntity : class;
