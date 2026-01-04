using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("api/crawlerTasks")]
[ApiController]
public class CrawlerTasksController(SqlDatabaseContext sqlDatabaseContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await sqlDatabaseContext.CrawlerTasks
            .OrderByDescending(task => task.Id)
            .ToArrayAsync(cancellationToken);

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CrawlerTaskCreateRequest request, CancellationToken cancellationToken = default)
    {
        var tag = await sqlDatabaseContext.ParsedTags
            .Where(t => t.Name == request.TagName)
            .FirstOrDefaultAsync(cancellationToken);
        if (tag == null)
            return BadRequest();

        var task = new CrawlerTask()
        {
            Type = request.Type,
            TagId = tag.Id,
            PostLineType = request.PostLineType,
            PageFrom = request.PageFrom,
            PageTo = request.PageTo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await sqlDatabaseContext.CrawlerTasks.AddAsync(task, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var task = await sqlDatabaseContext.CrawlerTasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
        if (task == null)
            return NotFound();

        sqlDatabaseContext.Remove(task);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}

public record CrawlerTaskCreateRequest
{
    public CrawlerTaskType Type { get; set; }
    public string TagName { get; set; }
    public PostLineType PostLineType { get; set; }
    public int? PageFrom { get; set; }
    public int? PageTo { get; set; }
}