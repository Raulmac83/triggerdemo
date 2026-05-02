using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triggers.db.Entities;
using triggers.repo.Notifications;

namespace triggers.api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsRepository _repo;

    public NotificationsController(INotificationsRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Notification>>> Recent([FromQuery] int take = 100, CancellationToken ct = default)
    {
        if (take is < 1 or > 1000) take = 100;
        var rows = await _repo.GetRecentAsync(take, ct);
        return Ok(rows);
    }

    [HttpDelete]
    public async Task<ActionResult<object>> ClearAll(CancellationToken ct)
    {
        var count = await _repo.ClearAllAsync(ct);
        return Ok(new { deleted = count });
    }
}
