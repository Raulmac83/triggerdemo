using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triggers.db.Entities;
using triggers.repo;

namespace triggers.api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repo;

    public CustomersController(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public record CustomerInput(string Name, string? Email, string? Phone, bool IsActive);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Customer>>> GetAll(CancellationToken ct)
        => Ok(await _repo.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id, CancellationToken ct)
    {
        var customer = await _repo.GetByIdAsync(id, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] CustomerInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return BadRequest(new { message = "Name is required." });

        var created = await _repo.CreateAsync(new Customer
        {
            Name = input.Name.Trim(),
            Email = input.Email,
            Phone = input.Phone,
            IsActive = input.IsActive,
        }, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Customer>> Update(int id, [FromBody] CustomerInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return BadRequest(new { message = "Name is required." });

        var updated = await _repo.UpdateAsync(id, new Customer
        {
            Name = input.Name.Trim(),
            Email = input.Email,
            Phone = input.Phone,
            IsActive = input.IsActive,
        }, ct);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _repo.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
