using System.Text.Json;
using triggers.events.domain;

namespace triggers.repo.Notifications;

public sealed record TriggerCreatedDomainEvent(int Id, string Name, bool IsEnabled) : IDomainEvent;
public sealed record TriggerUpdatedDomainEvent(int Id, string Name, bool IsEnabled) : IDomainEvent;
public sealed record TriggerDeletedDomainEvent(int Id, string Name) : IDomainEvent;

public class DomainTriggerCreatedHandler : IDomainEventHandler<TriggerCreatedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainTriggerCreatedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(TriggerCreatedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "TriggerAdded",
            "Trigger",
            e.Id,
            $"Trigger '{e.Name}' created",
            "Raised as a domain event from the Trigger entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}

public class DomainTriggerUpdatedHandler : IDomainEventHandler<TriggerUpdatedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainTriggerUpdatedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(TriggerUpdatedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "TriggerModified",
            "Trigger",
            e.Id,
            $"Trigger '{e.Name}' updated",
            "Raised as a domain event from the Trigger entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}

public class DomainTriggerDeletedHandler : IDomainEventHandler<TriggerDeletedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainTriggerDeletedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(TriggerDeletedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "TriggerDeleted",
            "Trigger",
            e.Id,
            $"Trigger '{e.Name}' deleted",
            "Raised as a domain event from the Trigger entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}
