using Microsoft.EntityFrameworkCore;
using triggers.db;
using triggers.db.Entities;
using triggers.repo.Notifications;

namespace triggers.repo;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        => await _db.Products.AsNoTracking().OrderByDescending(p => p.Id).ToListAsync(ct);

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        product.CreatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id, product.Name, product.Price));
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<Product?> UpdateAsync(int id, Product product, CancellationToken ct = default)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existing is null) return null;

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Sku = product.Sku;
        existing.Price = product.Price;
        existing.IsActive = product.IsActive;

        existing.RaiseDomainEvent(new ProductUpdatedDomainEvent(existing.Id, existing.Name, existing.Price));
        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existing is null) return false;

        existing.RaiseDomainEvent(new ProductDeletedDomainEvent(existing.Id, existing.Name));
        _db.Products.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
