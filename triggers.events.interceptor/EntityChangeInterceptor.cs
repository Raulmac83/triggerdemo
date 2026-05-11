using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace triggers.events.interceptor;

internal sealed record CapturedChange(
    EntityChangeType Type,
    object Entity,
    Type EntityClrType,
    IReadOnlyList<PropertyChange> Modified);

public class EntityChangeInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _rootProvider;
    private readonly List<CapturedChange> _pending = new();

    public EntityChangeInterceptor(IServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pending.Count == 0) return result;
        var snapshot = _pending.ToArray();
        _pending.Clear();
        await DispatchAsync(snapshot, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_pending.Count == 0) return result;
        var snapshot = _pending.ToArray();
        _pending.Clear();
        DispatchAsync(snapshot, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData) => _pending.Clear();

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _pending.Clear();
        return Task.CompletedTask;
    }

    private void Capture(DbContext? context)
    {
        if (context is null) return;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            var type = entry.State switch
            {
                EntityState.Added    => EntityChangeType.Added,
                EntityState.Modified => EntityChangeType.Modified,
                EntityState.Deleted  => EntityChangeType.Deleted,
                _                    => (EntityChangeType?)null,
            };
            if (type is null) continue;

            var changedProps = entry.State == EntityState.Modified
                ? entry.Properties
                    .Where(p => p.IsModified)
                    .Select(p => new PropertyChange(p.Metadata.Name, ReadOriginal(p), p.CurrentValue))
                    .ToList()
                : new List<PropertyChange>();

            _pending.Add(new CapturedChange(type.Value, entry.Entity, entry.Entity.GetType(), changedProps));
        }
    }

    private static object? ReadOriginal(PropertyEntry p)
    {
        try { return p.OriginalValue; } catch { return null; }
    }

    private async Task DispatchAsync(IReadOnlyList<CapturedChange> changes, CancellationToken ct)
    {
        using var scope = _rootProvider.CreateScope();
        var wildcardHandlers = scope.ServiceProvider.GetServices<IAnyEntityChangeHandler>().ToArray();

        foreach (var change in changes)
        {
            var openType = typeof(IEntityChangeHandler<>).MakeGenericType(change.EntityClrType);
            var handlers = (IEnumerable<object>)scope.ServiceProvider.GetServices(openType);

            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                var changeType = typeof(EntityChange<>).MakeGenericType(change.EntityClrType);
                var changeInstance = Activator.CreateInstance(changeType, change.Type, change.Entity, change.Modified)!;
                var method = openType.GetMethod(nameof(IEntityChangeHandler<object>.HandleAsync))!;
                var task = (Task)method.Invoke(handler, new[] { changeInstance, ct })!;
                await task.ConfigureAwait(false);
            }

            if (wildcardHandlers.Length > 0)
            {
                var any = new AnyEntityChange(change.Type, change.EntityClrType, change.Entity, change.Modified);
                foreach (var handler in wildcardHandlers)
                {
                    await handler.HandleAsync(any, ct).ConfigureAwait(false);
                }
            }
        }
    }
}
