using Microsoft.EntityFrameworkCore;
using triggers.db;
using triggers.db.Entities;
using triggers.repo.Notifications;

namespace triggers.repo;

public class TriggerRepository : ITriggerRepository
{
    private readonly AppDbContext _db;

    public TriggerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Trigger>> GetAllAsync(CancellationToken ct = default)
        => await _db.Triggers.AsNoTracking().OrderByDescending(t => t.Id).ToListAsync(ct);

    public Task<Trigger?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Triggers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Trigger> CreateAsync(Trigger trigger, CancellationToken ct = default)
    {
        trigger.CreatedAt = DateTime.UtcNow;
        _db.Triggers.Add(trigger);
        await _db.SaveChangesAsync(ct);
        // raise domain event after save so the row has its identity assigned
        trigger.RaiseDomainEvent(new TriggerCreatedDomainEvent(trigger.Id, trigger.Name, trigger.IsEnabled));
        await _db.SaveChangesAsync(ct);
        return trigger;
    }

    public async Task<Trigger?> UpdateAsync(int id, Trigger trigger, CancellationToken ct = default)
    {
        var existing = await _db.Triggers.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (existing is null) return null;

        existing.Name = trigger.Name;
        existing.Description = trigger.Description;
        existing.IsEnabled = trigger.IsEnabled;

        existing.RaiseDomainEvent(new TriggerUpdatedDomainEvent(existing.Id, existing.Name, existing.IsEnabled));
        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Triggers.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (existing is null) return false;

        existing.RaiseDomainEvent(new TriggerDeletedDomainEvent(existing.Id, existing.Name));
        _db.Triggers.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
