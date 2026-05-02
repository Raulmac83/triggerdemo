using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triggers.db.Entities;
using triggers.repo;

namespace triggers.api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TriggersController : ControllerBase
{
    private readonly ITriggerRepository _repo;

    public TriggersController(ITriggerRepository repo)
    {
        _repo = repo;
    }

    public record TriggerInput(string Name, string? Description, bool IsEnabled);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Trigger>>> GetAll(CancellationToken ct)
        => Ok(await _repo.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Trigger>> GetById(int id, CancellationToken ct)
    {
        var trigger = await _repo.GetByIdAsync(id, ct);
        return trigger is null ? NotFound() : Ok(trigger);
    }

    [HttpPost]
    public async Task<ActionResult<Trigger>> Create([FromBody] TriggerInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return BadRequest(new { message = "Name is required." });

        var created = await _repo.CreateAsync(new Trigger
        {
            Name = input.Name.Trim(),
            Description = input.Description,
            IsEnabled = input.IsEnabled,
        }, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Trigger>> Update(int id, [FromBody] TriggerInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return BadRequest(new { message = "Name is required." });

        var updated = await _repo.UpdateAsync(id, new Trigger
        {
            Name = input.Name.Trim(),
            Description = input.Description,
            IsEnabled = input.IsEnabled,
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
