# triggers.events.efcoretriggered

Thin wrapper + setup helpers around the **MIT-licensed** [`EntityFrameworkCore.Triggered`](https://github.com/koenbeuk/EFCoreTriggered) library. Provides a battle-tested before/after-save trigger pipeline with cascading-trigger support.

> **When to use:** you want a mature, maintained library that already solves edge cases (TPH, owned types, cascading saves, transaction boundaries). You're OK with one extra NuGet dependency. **License: MIT.**

## Pieces (provided by EntityFrameworkCore.Triggered)

| Type | Purpose |
|---|---|
| `IBeforeSaveTrigger<TEntity>` | runs before SaveChanges; can mutate the entity |
| `IAfterSaveTrigger<TEntity>` | runs after SaveChanges; ideal for side-effects (notifications, integration events) |
| `IAfterSaveFailedTrigger<TEntity>` | runs if the save fails |
| `ITriggerContext<TEntity>` | the change context — `ChangeType`, `Entity`, `UnmodifiedEntity` (the prior values) |

## Setup (3 steps)

### 1. Register triggers DI

```csharp
builder.Services.AddEfCoreTriggeredEvents();      // sets up EFCoreTriggered DI
builder.Services.AddAfterSaveTrigger<NotifyOnOrderChange>();
builder.Services.AddAfterSaveTrigger<NotifyOnUserChange>();
```

### 2. Hook the interceptor into your DbContext

```csharp
builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
{
    opts.UseSqlServer(connectionString);
    opts.UseEfCoreTriggeredEvents(sp);   // adds the trigger interceptor
});
```

### 3. Write a trigger

```csharp
using EntityFrameworkCore.Triggered;

public class NotifyOnOrderChange : IAfterSaveTrigger<Order>
{
    private readonly INotifier _notifier;
    public NotifyOnOrderChange(INotifier n) => _notifier = n;

    public Task OnAfterSave(ITriggerContext<Order> ctx, CancellationToken ct)
    {
        var summary = ctx.ChangeType switch
        {
            ChangeType.Added    => $"Order #{ctx.Entity.Id} placed",
            ChangeType.Modified => $"Order #{ctx.Entity.Id} updated",
            ChangeType.Deleted  => $"Order #{ctx.Entity.Id} cancelled",
            _ => string.Empty
        };
        return _notifier.SendAsync(summary, ct);
    }
}
```

## Semantics

- Triggers fire in registration order; one trigger can write changes that cascade into another.
- `ITriggerContext.UnmodifiedEntity` gives you a "before" snapshot — useful for diffing.
- Cascading is opt-in (see `TriggerOptions.MaxCascadeCycles`); disable to keep things simple.
- Trigger services are scoped — they can take a scoped `DbContext` and their writes will land in the same logical scope.

## When *not* to use this

- You want zero external deps → use `triggers.events.interceptor`.
- Your app doesn't tolerate dependency upgrades → pinning EFCoreTriggered to a specific EF Core minor version is required.

## License

MIT (this wrapper). EntityFrameworkCore.Triggered itself is also MIT.
