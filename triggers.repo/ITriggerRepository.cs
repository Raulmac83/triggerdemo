using triggers.db.Entities;

namespace triggers.repo;

public interface ITriggerRepository
{
    Task<IReadOnlyList<Trigger>> GetAllAsync(CancellationToken ct = default);
    Task<Trigger?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Trigger> CreateAsync(Trigger trigger, CancellationToken ct = default);
    Task<Trigger?> UpdateAsync(int id, Trigger trigger, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
