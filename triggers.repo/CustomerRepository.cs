using Microsoft.EntityFrameworkCore;
using triggers.db;
using triggers.db.Entities;
using triggers.repo.Notifications;

namespace triggers.repo;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
        => await _db.Customers.AsNoTracking().OrderByDescending(c => c.Id).ToListAsync(ct);

    public Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default)
    {
        customer.CreatedAt = DateTime.UtcNow;
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        customer.RaiseDomainEvent(new CustomerCreatedDomainEvent(customer.Id, customer.Name, customer.Email));
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task<Customer?> UpdateAsync(int id, Customer customer, CancellationToken ct = default)
    {
        var existing = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (existing is null) return null;

        existing.Name = customer.Name;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.IsActive = customer.IsActive;

        existing.RaiseDomainEvent(new CustomerUpdatedDomainEvent(existing.Id, existing.Name, existing.Email));
        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (existing is null) return false;

        existing.RaiseDomainEvent(new CustomerDeletedDomainEvent(existing.Id, existing.Name));
        _db.Customers.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
