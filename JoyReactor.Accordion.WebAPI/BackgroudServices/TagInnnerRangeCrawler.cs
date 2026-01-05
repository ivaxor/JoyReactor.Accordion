using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class TagInnnerRangeCrawler(
    IServiceScopeFactory serviceScopeFactory,

    ILogger<TagInnnerRangeCrawler> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
        await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
        var tagCrawler = serviceScope.ServiceProvider.GetRequiredService<ITagCrawler>();

        var take = 100;
        var skip = 0;

        var tagNumberIds = (HashSet<int>)null;
        var tagStartNumberId = 0;
        var tagEndNumberId = 0;
        do
        {
            tagNumberIds = await sqlDatabaseContext.ParsedTags
                .AsNoTracking()
                .OrderBy(tag => tag.NumberId)
                .Select(tag => tag.NumberId)
                .Skip(skip)
                .Take(take)
                .ToHashSetAsync(cancellationToken);
            if (tagNumberIds.Count == 0)
            {
                logger.LogInformation("No tags found. Will try again later");
                continue;
            }

            tagStartNumberId = tagNumberIds.First();
            tagEndNumberId = tagNumberIds.Last();

            var emptyTagNumberIds = await sqlDatabaseContext.EmptyTags
                .AsNoTracking()
                .Where(tag => tag.NumberId >= tagStartNumberId && tag.NumberId <= tagEndNumberId)
                .Select(tag => tag.NumberId)
                .ToHashSetAsync(cancellationToken);

            for (var tagNumberId = tagStartNumberId; tagNumberId <= tagEndNumberId; tagNumberId++)
            {
                if (tagNumberIds.Contains(tagNumberId) || emptyTagNumberIds.Contains(tagNumberId))
                    continue;

                var parsedTag = await tagCrawler.CrawlAsync(tagNumberId, cancellationToken);
                if (parsedTag == null)
                {
                    var emptyTag = new EmptyTag(tagNumberId);
                    await sqlDatabaseContext.EmptyTags.AddIgnoreExistingAsync(emptyTag, cancellationToken);
                    await sqlDatabaseContext.SaveChangesAsync();
                }
                else
                    skip++;
            }

            skip += take;
        } while (tagNumberIds.Count > 0);
    }
}