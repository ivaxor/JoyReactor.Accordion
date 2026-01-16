using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.WebAPI.Models.Requests;
using JoyReactor.Accordion.WebAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("crawlerTasks")]
[ApiController]
public class CrawlerTaskController(
    IMemoryCache memoryCache,
    SqlDatabaseContext sqlDatabaseContext)
    : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    [AllowAnonymous]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{nameof(CrawlerTaskController)}.{nameof(ListAsync)}";
        var response = await memoryCache.GetOrCreateAsync(cacheKey, cacheEntry => {
            cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(1);
            return GetCrawlerTasksAsync(cancellationToken);
        });        

        return Ok(response);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync([FromBody] CrawlerTaskCreateRequest request, CancellationToken cancellationToken = default)
    {
        var tag = await sqlDatabaseContext.ParsedTags
            .AsNoTracking()
            .Where(t => t.Name == request.TagName)
            .FirstOrDefaultAsync(cancellationToken);
        if (tag == null)
        {
            ModelState.AddModelError(nameof(request.TagName), "Tag with specified name not found");
            return BadRequest(ModelState);
        }

        var crawlerTask = new CrawlerTask()
        {
            TagId = tag.Id,
            PostLineType = request.PostLineType,
            PageCurrent = request.PageFrom,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await sqlDatabaseContext.CrawlerTasks.AddAsync(crawlerTask, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

        var response = new CrawlerTaskResponse(crawlerTask);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var crawlerTask = await sqlDatabaseContext.CrawlerTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
        if (crawlerTask == null)
            return NotFound();

        sqlDatabaseContext.CrawlerTasks.Remove(crawlerTask);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    protected async Task<CrawlerTaskResponse[]> GetCrawlerTasksAsync(CancellationToken cancellationToken)
    {
        var crawlerTasks = await sqlDatabaseContext.CrawlerTasks
            .AsNoTracking()
            .Include(crawlerTask => crawlerTask.Tag)
            .OrderByDescending(crawlerTask => crawlerTask.Id)
            .ToArrayAsync(cancellationToken);

        var response = crawlerTasks
            .Select(crawlerTask => new CrawlerTaskResponse(crawlerTask))
            .ToArray();

        return response;
    }
}