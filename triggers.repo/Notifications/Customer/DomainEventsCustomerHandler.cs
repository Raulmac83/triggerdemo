using System.Text.Json;
using triggers.events.domain;

namespace triggers.repo.Notifications;

public sealed record CustomerCreatedDomainEvent(int Id, string Name, string? Email) : IDomainEvent;
public sealed record CustomerUpdatedDomainEvent(int Id, string Name, string? Email) : IDomainEvent;
public sealed record CustomerDeletedDomainEvent(int Id, string Name) : IDomainEvent;

public class DomainCustomerCreatedHandler : IDomainEventHandler<CustomerCreatedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainCustomerCreatedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(CustomerCreatedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "CustomerAdded",
            "Customer",
            e.Id,
            $"Customer '{e.Name}' created",
            "Raised as a domain event from the Customer entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}

public class DomainCustomerUpdatedHandler : IDomainEventHandler<CustomerUpdatedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainCustomerUpdatedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(CustomerUpdatedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "CustomerModified",
            "Customer",
            e.Id,
            $"Customer '{e.Name}' updated",
            "Raised as a domain event from the Customer entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}

public class DomainCustomerDeletedHandler : IDomainEventHandler<CustomerDeletedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainCustomerDeletedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(CustomerDeletedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "CustomerDeleted",
            "Customer",
            e.Id,
            $"Customer '{e.Name}' deleted",
            "Raised as a domain event from the Customer entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}
