# triggers.events.interceptor

A tiny, dependency-free reusable trigger system built on EF Core's `SaveChangesInterceptor`.

> **When to use:** you want to react to *any* change made to an EF entity (Insert / Update / Delete) without coupling the entity to event-raising code. ~80 LOC, no third-party deps. **MIT-friendly: yours.**

## Pieces

| Type | Purpose |
|---|---|
| `EntityChange<TEntity>` | record passed to your handler — has `Type`, `Entity`, `ModifiedProperties` |
| `EntityChangeType` | enum `Added \| Modified \| Deleted` |
| `PropertyChange(Name, OldValue, NewValue)` | per-property diff (only populated for `Modified`) |
| `IEntityChangeHandler<TEntity>` | the one interface you implement |
| `EntityChangeInterceptor` | `SaveChangesInterceptor` that captures + dispatches |

## Setup (3 steps)

### 1. Register the interceptor and your handlers

```csharp
builder.Services.AddInterceptorTriggerEvents();

builder.Services.AddEntityChangeHandler<Order, NotifyOnOrderChange>();
builder.Services.AddEntityChangeHandler<User,  AuditUserChange>();
```

### 2. Wire the interceptor into your DbContext

```csharp
builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
{
    opts.UseSqlServer(connectionString);
    opts.UseInterceptorTriggerEvents(sp);
});
```

### 3. Write a handler

```csharp
public class NotifyOnOrderChange : IEntityChangeHandler<Order>
{
    private readonly INotifier _notifier;
    public NotifyOnOrderChange(INotifier n) => _notifier = n;

    public Task HandleAsync(EntityChange<Order> change, CancellationToken ct)
    {
        var summary = change.Type switch
        {
            EntityChangeType.Added    => $"Order #{change.Entity.Id} placed",
            EntityChangeType.Modified => $"Order #{change.Entity.Id} updated ({change.ModifiedProperties.Count} fields)",
            EntityChangeType.Deleted  => $"Order #{change.Entity.Id} cancelled",
            _ => string.Empty
        };
        return _notifier.SendAsync(summary, ct);
    }
}
```

## Single-entity vs multi-entity triggers

Implementing `IEntityChangeHandler<TEntity>` directly is fine for one-off handlers, but two
base classes make the common cases shorter and let one trigger watch a *list* of entity
types in a single class.

### Single entity — `EntityTrigger<TEntity>`

```csharp
public class NotifyOrder : EntityTrigger<Order>
{
    private readonly INotifier _notifier;
    public NotifyOrder(INotifier n) => _notifier = n;

    protected override Task OnChangedAsync(
        EntityChangeType kind,
        Order entity,
        IReadOnlyList<PropertyChange> modifiedProperties,
        CancellationToken ct)
    {
        return _notifier.SendAsync($"Order #{entity.Id} {kind}", ct);
    }
}

// registration
builder.Services.AddEntityTrigger<NotifyOrder>();
```

`AddEntityTrigger<T>()` registers the class as scoped and wires every
`IEntityChangeHandler<X>` (and `IAnyEntityChangeHandler`) it implements — so the same call
works for the single- and multi-entity flavours below.

### Multi-entity — `EntityTrigger<T1, T2[, T3, T4]>`

```csharp
public class NotifyAny : EntityTrigger<Order, User, Trigger>
{
    protected override Task OnChangedAsync(
        EntityChangeType kind,
        object entity,
        IReadOnlyList<PropertyChange> modifiedProperties,
        CancellationToken ct)
    {
        var label = entity switch
        {
            Order o   => $"Order #{o.Id}",
            User u    => $"User {u.Username}",
            Trigger t => $"Trigger '{t.Name}'",
            _         => entity.GetType().Name,
        };
        Console.WriteLine($"{label} {kind}");
        return Task.CompletedTask;
    }
}

builder.Services.AddEntityTrigger<NotifyAny>();
```

The handler receives the changed entity as `object`; pattern-match if you need typed
access. C# doesn't have variadic generics, so generic overloads exist for 1, 2, 3, and 4
types. For 5+, derive directly from `MultiEntityTriggerBase` and override `WatchedTypes`:

```csharp
public class WatchEverything : MultiEntityTriggerBase
{
    protected override IReadOnlyList<Type> WatchedTypes { get; } =
        new[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E), typeof(F) };

    protected override Task OnChangedAsync(
        EntityChangeType kind, object entity,
        IReadOnlyList<PropertyChange> modifiedProperties, CancellationToken ct)
    {
        // ...
    }
}
```

Under the hood, multi-entity triggers implement `IAnyEntityChangeHandler` — a wildcard
contract the interceptor dispatches to alongside the typed
`IEntityChangeHandler<TEntity>` handlers. Both fire for the same change, so a multi-entity
trigger and a per-entity trigger watching the same type will each see their own
notification.

## Semantics

- Handlers run **after** the EF transaction commits (in `SavedChangesAsync`). On failure, captured changes are dropped.
- Handlers resolve from a **fresh DI scope**, so a handler can use its own `DbContext` without re-entering this interceptor.
- Multiple handlers per entity are allowed; they run sequentially in registration order.
- Property diffs are only captured for `Modified` entries (Added has no "before"; Deleted has no "after").
- Bulk operations (`ExecuteUpdate`, `ExecuteDelete`) bypass the change tracker — use a DB trigger or repository wrapper for those.

## Testing

```csharp
public class FakeHandler : IEntityChangeHandler<Order>
{
    public List<EntityChange<Order>> Captured { get; } = new();
    public Task HandleAsync(EntityChange<Order> c, CancellationToken ct) {
        Captured.Add(c); return Task.CompletedTask;
    }
}

// Register the fake in a test ServiceProvider, run a SaveChangesAsync, assert.
```

## When *not* to use this

- You want events to carry *intent* (e.g. `OrderShipped`, not generic "Order modified") → use `triggers.events.domain` instead.
- You need cross-process eventing → emit from a handler into a bus.
- You need full audit history → use `triggers.events.audit`.

## License

MIT — copy-pastable into your project, no notice needed.
