# `triggers.events` — Planning Doc

A small, reusable library that hooks `EF Core SaveChanges` and dispatches in-process events when rows in a registered entity change. Goal is "simple, not over-engineered, drop into any app."

## Goals

- One place to say "fire X when entity Y changes" without touching repo/controller code.
- Reusable across this project (and any other EF Core 8+ project) as a NuGet/project reference.
- Capture **what** changed: entity, change type (Insert/Update/Delete), and modified property diff (old → new).
- Async, supports DI'd services in handlers (so a handler can write a `Notification` row, send an email, publish to a bus, etc).
- Handlers run **after** `SaveChanges` succeeds — never on a rolled-back transaction.

## Non-goals (for v1)

- Cross-process / cross-service eventing (no Kafka, no RabbitMQ — that's a later layer on top).
- Persistent outbox / retry-on-failure (can be added once the in-process surface is stable).
- Versioned event schemas, replay, projections.
- Capturing changes that bypass EF (raw SQL `UPDATE`, Flyway migrations, etc — those need DB triggers).

---

## Option A — Hand-rolled `SaveChangesInterceptor` + handler interface (recommended)

Build a tiny library directly on top of EF Core's built-in `ISaveChangesInterceptor`. ~80 LOC of core code.

**Public surface**
```csharp
// 1. Mark the interesting entities and write handlers per type.
public interface IEntityChangeHandler<TEntity> where TEntity : class
{
    Task HandleAsync(EntityChange<TEntity> change, CancellationToken ct);
}

public enum EntityChangeType { Added, Modified, Deleted }

public sealed record PropertyChange(string Name, object? OldValue, object? NewValue);

public sealed record EntityChange<TEntity>(
    EntityChangeType Type,
    TEntity Entity,
    IReadOnlyList<PropertyChange> ModifiedProperties);

// 2. Wire up in Program.cs
builder.Services
    .AddTriggerEvents()
    .AddEntityHandler<Trigger, NotifyOnTriggerChange>()
    .AddEntityHandler<User,    SyncUserToCrm>();

// 3. The DbContext registers the interceptor automatically:
builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
    opts.UseSqlServer(connStr).AddTriggerEventInterceptor(sp));
```

**How it works**

1. `EntityChangeInterceptor : SaveChangesInterceptor` overrides `SavingChangesAsync` — walks `ChangeTracker.Entries()`, snapshots each `Added/Modified/Deleted` entry into a per-`DbContext`-instance list (capturing `OriginalValues` *now*, before they get reset).
2. Overrides `SavedChangesAsync` — once the SQL transaction commits, drains the list and dispatches each change to `IEnumerable<IEntityChangeHandler<TEntity>>` resolved from `IServiceProvider`.
3. Failures in handlers are logged but don't roll back the save (ordering is "save first, react after"). Optional: collect handler exceptions and surface as `AggregateException`.

**Pros**
- Zero third-party deps. MIT-friendly by definition (your code).
- ~80 LOC core; you can read the whole library on one screen.
- No magic — handlers are just DI services.
- Trivial to test (instantiate interceptor + fake handler).

**Cons**
- You own it. Edge cases (TPH inheritance, owned types, shadow properties, value converters) need explicit thought.
- No "before save" hooks (intentional for v1; can add `IBeforeSaveHandler<T>` later).

**Effort:** small. ~half a day to ship `triggers.events` + tests + sample handler.

---

## Option B — `EFCore.Triggered` ([koenbeuk/EFCoreTriggered](https://github.com/koenbeuk/EFCoreTriggered))

Mature, MIT-licensed, purpose-built. Adds a trigger pipeline to `SaveChanges` with `IBeforeSaveTrigger<T>` / `IAfterSaveTrigger<T>` / `IAfterSaveFailedTrigger<T>` and cascading-trigger support.

**Sketch**
```csharp
public class TriggerChangedHandler : IAfterSaveTrigger<Trigger>
{
    public Task OnAfterSave(ITriggerContext<Trigger> ctx, CancellationToken ct) =>
        ctx.ChangeType switch
        {
            ChangeType.Added    => /* write Notification */,
            ChangeType.Modified => /* write Notification, ctx.UnmodifiedEntity has prior */,
            ChangeType.Deleted  => /* write Notification */,
            _                   => Task.CompletedTask,
        };
}

services.AddTriggers(t => t.AddTrigger<TriggerChangedHandler>());
optionsBuilder.UseTriggers();
```

**Pros**
- Solves all the EF edge cases for you (cascading triggers, owned types, transactions, "trigger from a trigger").
- MIT, actively maintained, 1k+ stars, proven in production.
- Before/after/failure hooks out of the box.

**Cons**
- New dependency, new vocabulary (`ITrigger`, `ITriggerContext`).
- More machinery than you need for v1.
- "Cascading triggers" can re-enter `SaveChanges` and surprise you if you're not careful.

**Effort:** smaller than A in code, larger in "library on the shelf" surface area.

---

## Option C — Domain Events on the entity + dispatcher in interceptor

Classic DDD pattern. Entities accumulate events in a private list; an interceptor drains and dispatches them.

```csharp
public abstract class Entity {
    private readonly List<object> _events = new();
    public IReadOnlyList<object> DomainEvents => _events;
    protected void Raise(object e) => _events.Add(e);
    public void ClearEvents() => _events.Clear();
}

public class Trigger : Entity {
    public void Disable() {
        IsEnabled = false;
        Raise(new TriggerDisabled(Id));
    }
}

services.AddScoped<IDomainEventDispatcher, ServiceProviderDispatcher>();
```

Dispatcher uses `IServiceProvider.GetServices<IEventHandler<TEvent>>()` and runs them after save.

**Pros**
- Events are *intentional* — entities document the things that happen to them.
- Decouples "what happened" (event type) from "how I detected it" (EF change tracking).
- Plays nicely with CQRS, easier to test, no diffing needed.

**Cons**
- Requires every interesting entity to inherit / opt-in. Doesn't react to *anonymous* changes (e.g. a third-party service updates the row directly through EF).
- A bit more ceremony per change site.
- Conflicts with the user's stated goal — "react when *anything* changes the row" is an inversion of control that domain events deliberately avoid.

**Note on libraries:** the canonical pairing was `MediatR`, but MediatR moved to a **commercial license** in late 2024 (v12.x is the last MIT version). Replacements:
- **Mediator** by Martin Othamar — MIT, source-generated, fast: https://github.com/martinothamar/Mediator
- **Brighter** — Apache 2.0: https://github.com/BrighterCommand/Brighter
- Or a 30-line hand-rolled `IDomainEventDispatcher`.

---

## Option D — `Audit.NET` + `Audit.EntityFramework`

[`thepirat000/Audit.NET`](https://github.com/thepirat000/Audit.NET), MIT. Drop-in EF auditing. Subscribes to `SavingChanges`, captures full row diffs, lets you route the audit record to any sink (DB, JSON file, event handler).

**Pros**
- Zero-config audit table; rich diff format already designed.
- Production-grade; rows include user, request id, etc.
- If the eventual goal is also "show me history of every change" (which the new `Notifications` table flirts with), this gets you both.

**Cons**
- More than the user asked for. Closer to "audit log" than "trigger an event."
- Schema is theirs; integrating with bespoke `Notifications` table means writing a custom `DataProvider`.
- Doesn't directly give you a typed `IHandler<TEntity>` story — you'd build one on top.

---

## Comparison

| | A. Hand-rolled | B. EFCore.Triggered | C. Domain events + dispatcher | D. Audit.NET |
|---|---|---|---|---|
| License | yours | MIT | MIT (Mediator if used) | MIT |
| External deps | none | 1 | 0–1 | 1+ |
| LOC to maintain | ~80 | ~0 (config) | ~50 + per-entity ceremony | ~0 (config) + custom sink |
| Reacts to *any* EF change | ✅ | ✅ | ❌ (only when entity raises) | ✅ |
| Property diff | hand-built (easy with EF API) | provided | n/a (events carry intent) | provided + verbose |
| Before-save hooks | not in v1 | ✅ | possible | ✅ |
| Reusable across apps | NuGet from you | already a NuGet | per-app pattern | NuGet |
| Surprise factor | low | medium (cascading triggers) | low | medium (audit semantics) |

---

## Recommendation

**Go with Option A.** It directly matches the brief — small, no deps, easy to drop into any EF Core project, and exposes a focused `IEntityChangeHandler<T>` surface anyone can implement.

If A starts growing edge-case handling (TPH, owned types, cascading saves, retry-on-failure), revisit B as the upgrade path — the public shape is similar enough that handlers can be ported.

C is the right choice **only if** the entities should encode intent ("trigger was disabled by user" vs "row was updated"). For an event-on-any-change reusable hook, it's the wrong shape.

D is right if you also want a generic audit log. If that becomes a goal, layer A on top of D rather than replacing.

---

## Proposed `triggers.events` shape (Option A, concrete)

**Project:** new `.NET 8` class library `triggers.events`, referenced by `triggers.db` (which owns the `DbContext`) and by any consumer that wants to register handlers.

```
triggers.events/
├── EntityChangeType.cs           // enum: Added | Modified | Deleted
├── PropertyChange.cs             // record (Name, OldValue, NewValue)
├── EntityChange.cs               // record EntityChange<TEntity>(Type, Entity, ModifiedProperties)
├── IEntityChangeHandler.cs       // Task HandleAsync(EntityChange<TEntity>, CancellationToken)
├── EntityChangeInterceptor.cs    // ISaveChangesInterceptor implementation
├── DependencyInjection.cs        // AddTriggerEvents(), AddEntityHandler<TEntity, THandler>()
└── DbContextOptionsBuilderExtensions.cs  // .AddTriggerEventInterceptor(sp)
```

**Consumer wiring (in `triggers.api/Program.cs`)**
```csharp
builder.Services.AddTriggerEvents()
    .AddEntityHandler<Trigger, NotifyOnTriggerChange>();

builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
    opts.UseSqlServer(connStr).AddTriggerEventInterceptor(sp));
```

**Handler example (lives in `triggers.repo` or a new `triggers.notifications` module)**
```csharp
public class NotifyOnTriggerChange : IEntityChangeHandler<Trigger>
{
    private readonly AppDbContext _db;
    public NotifyOnTriggerChange(AppDbContext db) => _db = db;

    public async Task HandleAsync(EntityChange<Trigger> change, CancellationToken ct)
    {
        var (type, entity, props) = (change.Type, change.Entity, change.ModifiedProperties);
        _db.Notifications.Add(new Notification {
            Type       = $"Trigger{type}",
            EntityType = nameof(Trigger),
            EntityId   = entity.Id,
            Title      = $"Trigger '{entity.Name}' was {type.ToString().ToLower()}",
            Payload    = JsonSerializer.Serialize(props),
        });
        await _db.SaveChangesAsync(ct);
    }
}
```

> ⚠️ Note: the handler calls `SaveChangesAsync` again, which will re-enter the interceptor. The implementation guards against re-entry (skip dispatch when the interceptor is already mid-dispatch). Alternative: use a separate `DbContext` for handler writes — cleaner separation, no re-entry.

---

## Open questions to resolve before implementation

1. **Sync vs async dispatch.** v1 = await each handler in registration order. Should one handler's failure stop the rest? Suggest: log + continue, surface `AggregateException` at end (configurable).
2. **Scope of handler resolution.** Use the same scoped `DbContext` (re-entry risk above) or always create a fresh scope per dispatch? Suggest: fresh scope, opt-out via flag for cases where the handler should see uncommitted state.
3. **Property diff for "Added" / "Deleted".** For Added, "new value" only (old is null). For Deleted, "old value" only. Settle on a consistent shape.
4. **Soft-deletes.** If we add a global query filter for `IsDeleted`, do soft-delete writes appear as `Modified` or `Deleted`? Likely `Modified`. Document this.
5. **Bulk operations.** EF Core 7+ `ExecuteUpdate`/`ExecuteDelete` bypass change tracker. Document as "won't fire" and recommend DB triggers for those paths.
6. **Test surface.** Need a tiny in-memory SQLite test harness in the library to prove the interceptor wires up correctly.

---

## Suggested rollout

1. Scaffold `triggers.events` class library (no deps beyond `Microsoft.EntityFrameworkCore`).
2. Implement core types + interceptor + DI + a unit test using SQLite in-memory.
3. Wire `triggers.api` to register the interceptor against `AppDbContext`.
4. Re-scaffold or hand-add `Notification` entity to `AppDbContext`.
5. Implement `NotifyOnTriggerChange` handler — write a row to `Notifications` per change.
6. Surface `Notifications` in the API (`GET /api/notifications`) and UI (bell icon in AppBar with unread count).

Step 1–3 deliver the reusable mechanism; 4–6 prove it on the `Trigger` table per the original ask.
