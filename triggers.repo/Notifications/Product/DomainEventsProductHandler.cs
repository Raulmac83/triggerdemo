using System.Text.Json;
using triggers.events.domain;

namespace triggers.repo.Notifications;

public sealed record ProductCreatedDomainEvent(int Id, string Name, decimal Price) : IDomainEvent;
public sealed record ProductUpdatedDomainEvent(int Id, string Name, decimal Price) : IDomainEvent;
public sealed record ProductDeletedDomainEvent(int Id, string Name) : IDomainEvent;

public class DomainProductCreatedHandler : IDomainEventHandler<ProductCreatedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainProductCreatedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(ProductCreatedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "ProductAdded",
            "Product",
            e.Id,
            $"Product '{e.Name}' created",
            "Raised as a domain event from the Product entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}

public class DomainProductUpdatedHandler : IDomainEventHandler<ProductUpdatedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainProductUpdatedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(ProductUpdatedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "ProductModified",
            "Product",
            e.Id,
            $"Product '{e.Name}' updated",
            "Raised as a domain event from the Product entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}

public class DomainProductDeletedHandler : IDomainEventHandler<ProductDeletedDomainEvent>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public DomainProductDeletedHandler(INotificationWriter w, ITriggerMethodSelector s) { _writer = w; _selector = s; }

    public Task HandleAsync(ProductDeletedDomainEvent e, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.DomainEvents) return Task.CompletedTask;
        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.DomainEvents,
            "ProductDeleted",
            "Product",
            e.Id,
            $"Product '{e.Name}' deleted",
            "Raised as a domain event from the Product entity.",
            JsonSerializer.Serialize(e)), ct);
    }
}
