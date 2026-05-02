# triggers.events.audit

Trigger events powered by **MIT-licensed** [`Audit.NET`](https://github.com/thepirat000/Audit.NET) + `Audit.EntityFramework.Core`. Captures full row diffs (with old / new values), normalizes them into `AuditTriggerEvent`, and dispatches to your handlers.

> **When to use:** you also want a structured audit story — old/new values for every column, even on entities you didn't anticipate. **License: MIT.**

## Pieces

| Type | Purpose |
|---|---|
| `AuditTriggerEvent(Action, EntityName, PrimaryKey, Entity, Changes)` | normalized event passed to handlers |
| `AuditColumnChange(OldValue, NewValue)` | per-column diff |
| `IAuditTriggerHandler` | implement this to react |
| `AuditTriggerDataProvider` | bridges Audit.NET → your handlers |

## Setup (4 steps)

### 1. Register services

```csharp
builder.Services.AddAuditTriggerEvents();
builder.Services.AddAuditTriggerHandler<NotifyOnAnyChange>();
```

### 2. Hook the EF interceptor into your DbContext

```csharp
builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
{
    opts.UseSqlServer(connectionString);
    opts.UseAuditInterceptor();
});
```

### 3. Activate Audit.NET globally

```csharp
var app = builder.Build();
app.Services.UseAuditTriggerEvents();   // call once after services are built
```

### 4. Write a handler

```csharp
public class NotifyOnAnyChange : IAuditTriggerHandler
{
    private readonly INotifier _notifier;
    public NotifyOnAnyChange(INotifier n) => _notifier = n;

    // Optional filter: only react to one entity type
    public bool ShouldHandle(AuditTriggerEvent evt) => evt.EntityName == "Order";

    public Task HandleAsync(AuditTriggerEvent evt, CancellationToken ct)
    {
        var summary = $"{evt.Action} on {evt.EntityName}#{evt.PrimaryKey} " +
                      $"({string.Join(", ", evt.Changes.Keys)})";
        return _notifier.SendAsync(summary, ct);
    }
}
```

## Semantics

- The interceptor records before/after values for every changed column. `Changes` is a dictionary keyed by column name with old + new values.
- `Action` is one of `"Insert"`, `"Update"`, `"Delete"`.
- Handlers run **after** the EF transaction commits. Failed saves don't dispatch.
- Handlers resolve from a fresh DI scope (so they can use a scoped `DbContext` without re-entering this pipeline).

## When *not* to use this

- You don't need full row diffs and want the smallest dependency footprint → `triggers.events.interceptor`.
- Audit.NET's static `Audit.Core.Configuration` global state isn't acceptable in your hosting model.

## License

MIT (this wrapper). Audit.NET itself is also MIT.
