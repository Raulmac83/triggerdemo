using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using triggers.db.Entities;
using triggers.events.efcoretriggered;

namespace triggers.repo.Notifications;

/// <summary>
/// Uses the new <see cref="EntityTrigger{TEntity}"/> base from triggers.events.efcoretriggered.
/// EFCoreTriggered handlers are scoped to the DbContext's scope; opening a child scope here
/// matches the existing trigger.Triggers pipeline pattern so writes go through the same
/// services the request-scope is using.
/// </summary>
public class EFCoreTriggeredProductHandler : EntityTrigger<Product>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EFCoreTriggeredProductHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task OnChangedAsync(
        EntityChangeKind kind,
        Product entity,
        Product? unmodifiedEntity,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var selector = scope.ServiceProvider.GetRequiredService<ITriggerMethodSelector>();
        var writer   = scope.ServiceProvider.GetRequiredService<INotificationWriter>();
        if (selector.Current != TriggerMethod.EFCoreTriggered) return;

        var verb = kind switch
        {
            EntityChangeKind.Added    => "created",
            EntityChangeKind.Modified => "updated",
            EntityChangeKind.Deleted  => "deleted",
            _ => "changed",
        };

        await writer.WriteAsync(new NotificationInput(
            TriggerMethod: TriggerMethodNames.EFCoreTriggered,
            Type: $"Product{kind}",
            EntityType: nameof(Product),
            EntityId: entity.Id,
            Title: $"Product '{entity.Name}' {verb}",
            Message: "Captured by EntityFrameworkCore.Triggered (via EntityTrigger<Product>).",
            Payload: JsonSerializer.Serialize(new
            {
                changeType = kind.ToString(),
                entity = new { entity.Id, entity.Name, entity.Sku, entity.Price, entity.IsActive },
            })), cancellationToken);
    }
}
