# triggers.events.domain

Classic DDD-style domain events on top of EF Core, dispatched after `SaveChanges` succeeds.

> **When to use:** events should encode *intent* — `OrderShipped`, `UserPasswordReset`, `TriggerDisabledByAdmin` — not generic "row changed". The entity decides which events to raise; handlers react. **License: MIT.**

## Pieces

| Type | Purpose |
|---|---|
| `IDomainEvent` | marker interface for your event records |
| `IEventfulEntity` | implement on entities that raise events |
| `DomainEventCollector` | optional helper to compose into your entity |
| `IDomainEventHandler<TEvent>` | the handler interface |
| `DomainEventInterceptor` | drains entities and dispatches after save |

## Setup (4 steps)

### 1. Define an event

```csharp
public sealed record OrderShipped(int OrderId, DateTime ShippedAt) : IDomainEvent;
```

### 2. Make your entity raise it

```csharp
public partial class Order : IEventfulEntity
{
    private readonly DomainEventCollector _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.Events;
    public void ClearDomainEvents() => _events.Clear();

    public void Ship()
    {
        Status = OrderStatus.Shipped;
        _events.Raise(new OrderShipped(Id, DateTime.UtcNow));
    }
}
```

### 3. Write the handler

```csharp
public class WhenOrderShipped : IDomainEventHandler<OrderShipped>
{
    private readonly INotifier _notifier;
    public WhenOrderShipped(INotifier n) => _notifier = n;

    public Task HandleAsync(OrderShipped e, CancellationToken ct) =>
        _notifier.SendAsync($"Order {e.OrderId} shipped at {e.ShippedAt:O}", ct);
}
```

### 4. Wire DI + DbContext

```csharp
builder.Services.AddDomainEventTriggers();
builder.Services.AddDomainEventHandler<OrderShipped, WhenOrderShipped>();

builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
{
    opts.UseSqlServer(connectionString);
    opts.UseDomainEventTriggers(sp);
});
```

## Semantics

- The interceptor walks `ChangeTracker.Entries()`, extracts `DomainEvents` from any `IEventfulEntity`, and clears them so they don't re-fire.
- Events dispatch **after** the SQL transaction commits, in a fresh DI scope.
- Multiple handlers per event are allowed; they run sequentially in registration order.
- Failed save → pending events dropped. Add an outbox table if you need at-least-once delivery.

## When *not* to use this

- You want to react to changes that **don't** raise events (e.g. someone updated the row through a different code path) → use `triggers.events.interceptor` or `triggers.events.audit`.
- You don't want entities to know about events → use a different pattern.

## License

MIT.
