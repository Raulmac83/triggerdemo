using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using triggers.repo.Notifications;

namespace triggers.api.Controllers;

[ApiController]
[Authorize]
[Route("api/trigger-methods")]
public class TriggerMethodsController : ControllerBase
{
    private static readonly TriggerMethodInfo[] _methods = new[]
    {
        new TriggerMethodInfo(
            Id: TriggerMethodNames.Interceptor,
            Name: "EF Core SaveChangesInterceptor",
            Library: "triggers.events.interceptor",
            Tagline: "Hand-rolled, dependency-free. ~80 LOC.",
            Highlights: new[] { "Zero third-party deps", "Reacts to any EF change", "Per-property diff" }),
        new TriggerMethodInfo(
            Id: TriggerMethodNames.EFCoreTriggered,
            Name: "EntityFrameworkCore.Triggered",
            Library: "triggers.events.efcoretriggered",
            Tagline: "MIT library by koenbeuk. Battle-tested before/after triggers.",
            Highlights: new[] { "Cascading triggers", "Before/after/failure hooks", "Production-grade" }),
        new TriggerMethodInfo(
            Id: TriggerMethodNames.DomainEvents,
            Name: "Domain Events",
            Library: "triggers.events.domain",
            Tagline: "Entities raise intent-named events; dispatcher fires them after save.",
            Highlights: new[] { "Encodes intent (e.g. TriggerCreated)", "DDD-aligned", "No reaction to anonymous changes" }),
        new TriggerMethodInfo(
            Id: TriggerMethodNames.AuditNet,
            Name: "Audit.NET",
            Library: "triggers.events.audit",
            Tagline: "MIT library by thepirat000. Full row diffs, audit-friendly.",
            Highlights: new[] { "Old + new values per column", "Useful for compliance/audit logs", "Static config" }),
    };

    private readonly ITriggerMethodSelector _selector;

    public TriggerMethodsController(ITriggerMethodSelector selector)
    {
        _selector = selector;
    }

    [HttpGet]
    public ActionResult<IEnumerable<TriggerMethodInfo>> List() => Ok(_methods);

    [HttpGet("active")]
    public ActionResult<ActiveMethodResponse> Active()
        => Ok(new ActiveMethodResponse(_selector.Current.ToName()));

    [HttpPut("active")]
    public ActionResult<ActiveMethodResponse> SetActive([FromBody] ActiveMethodRequest body)
    {
        if (!TriggerMethodNames.TryParse(body.Method, out var parsed))
            return BadRequest(new { message = $"Unknown method '{body.Method}'." });
        _selector.Set(parsed);
        return Ok(new ActiveMethodResponse(parsed.ToName()));
    }

    [HttpGet("{id}/docs")]
    [Produces("text/markdown")]
    public ActionResult Docs(string id)
    {
        var (assemblyName, resourceName) = id switch
        {
            TriggerMethodNames.Interceptor     => ("triggers.events.interceptor",     "triggers.events.interceptor.README.md"),
            TriggerMethodNames.EFCoreTriggered => ("triggers.events.efcoretriggered", "triggers.events.efcoretriggered.README.md"),
            TriggerMethodNames.DomainEvents    => ("triggers.events.domain",          "triggers.events.domain.README.md"),
            TriggerMethodNames.AuditNet        => ("triggers.events.audit",           "triggers.events.audit.README.md"),
            _ => (null, null),
        };
        if (assemblyName is null) return NotFound();

        var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName)
                  ?? Assembly.Load(assemblyName);
        using var stream = asm.GetManifestResourceStream(resourceName!);
        if (stream is null) return NotFound();
        using var reader = new StreamReader(stream);
        var markdown = reader.ReadToEnd();
        return Content(markdown, "text/markdown");
    }
}

public record TriggerMethodInfo(string Id, string Name, string Library, string Tagline, string[] Highlights);
public record ActiveMethodResponse(string Method);
public record ActiveMethodRequest(string Method);
