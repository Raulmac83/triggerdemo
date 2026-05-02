using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace triggers.events.domain;

public class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _rootProvider;
    private readonly List<IDomainEvent> _pending = new();

    public DomainEventInterceptor(IServiceProvider rootProvider)
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

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
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
            if (entry.Entity is IEventfulEntity eventful && eventful.DomainEvents.Count > 0)
            {
                _pending.AddRange(eventful.DomainEvents);
                eventful.ClearDomainEvents();
            }
        }
    }

    private async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken ct)
    {
        using var scope = _rootProvider.CreateScope();
        foreach (var evt in events)
        {
            var openType = typeof(IDomainEventHandler<>).MakeGenericType(evt.GetType());
            var handlers = (IEnumerable<object>)scope.ServiceProvider.GetServices(openType);
            var method = openType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                var task = (Task)method.Invoke(handler, new object[] { evt, ct })!;
                await task.ConfigureAwait(false);
            }
        }
    }
}
