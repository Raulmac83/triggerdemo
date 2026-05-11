using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using triggers.db.Entities;
using triggers.events.efcoretriggered;

namespace triggers.repo.Notifications;

public class EFCoreTriggeredCustomerHandler : EntityTrigger<Customer>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EFCoreTriggeredCustomerHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task OnChangedAsync(
        EntityChangeKind kind,
        Customer entity,
        Customer? unmodifiedEntity,
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
            Type: $"Customer{kind}",
            EntityType: nameof(Customer),
            EntityId: entity.Id,
            Title: $"Customer '{entity.Name}' {verb}",
            Message: "Captured by EntityFrameworkCore.Triggered (via EntityTrigger<Customer>).",
            Payload: JsonSerializer.Serialize(new
            {
                changeType = kind.ToString(),
                entity = new { entity.Id, entity.Name, entity.Email, entity.IsActive },
            })), cancellationToken);
    }
}
