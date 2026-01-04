using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class TagSubTagsCrawler(
    SqlDatabaseContext sqlDatabaseContext,
    ITagCrawler tagCrawler,
    IOptions<CrawlerSettings> settings,
    ILogger<TagSubTagsCrawler> logger)
    : ScopedBackgroudService
{
    internal readonly PeriodicTimer PeriodicTimer = new PeriodicTimer(settings.Value.SubsequentRunDelay);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var tagsWithEmptySubTags = (ParsedTag[])null;
        do
        {
            tagsWithEmptySubTags = await sqlDatabaseContext.ParsedTags
                .Where(tag => tag.MainTagId == null && tag.SubTagsCount > 0 && tag.SubTags.Count() < tag.SubTagsCount)
                .OrderByDescending(tag => tag.Id)
                .Take(100)
                .ToArrayAsync(cancellationToken);

            if (tagsWithEmptySubTags.Length != 0)
                logger.LogInformation("Crawling {TagsCount} tags for new sub tags", tagsWithEmptySubTags.Count());
            else
            {
                logger.LogInformation("No tags without sub tags found. Will try again later");
                continue;
            }

            foreach (var parsedTag in tagsWithEmptySubTags)
                await tagCrawler.CrawlSubTagsAsync(parsedTag.NumberId, cancellationToken);
        } while (tagsWithEmptySubTags.Length != 0 || await PeriodicTimer.WaitForNextTickAsync(cancellationToken));
    }
}