using System.Text.Json;
using EntityFrameworkCore.Triggered;
using Microsoft.Extensions.DependencyInjection;
using triggers.db.Entities;

namespace triggers.repo.Notifications;

public class EfCoreTriggeredTriggerHandler : IAfterSaveTrigger<Trigger>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EfCoreTriggeredTriggerHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task AfterSave(ITriggerContext<Trigger> context, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var selector = scope.ServiceProvider.GetRequiredService<ITriggerMethodSelector>();
        var writer = scope.ServiceProvider.GetRequiredService<INotificationWriter>();
        if (selector.Current != TriggerMethod.EFCoreTriggered) return;

        var verb = context.ChangeType switch
        {
            ChangeType.Added    => "created",
            ChangeType.Modified => "updated",
            ChangeType.Deleted  => "deleted",
            _ => "changed",
        };

        await writer.WriteAsync(new NotificationInput(
            TriggerMethod: TriggerMethodNames.EFCoreTriggered,
            Type: $"Trigger{context.ChangeType}",
            EntityType: nameof(Trigger),
            EntityId: context.Entity.Id,
            Title: $"Trigger '{context.Entity.Name}' {verb}",
            Message: "Captured by EntityFrameworkCore.Triggered.",
            Payload: JsonSerializer.Serialize(new
            {
                changeType = context.ChangeType.ToString(),
                entity = new { context.Entity.Id, context.Entity.Name, context.Entity.IsEnabled },
            })), cancellationToken);
    }
}
