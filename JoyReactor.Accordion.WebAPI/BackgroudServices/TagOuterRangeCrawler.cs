using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class TagOuterRangeCrawler(
    SqlDatabaseContext sqlDatabaseContext,
    ITagCrawler tagCrawler,
    IOptions<CrawlerSettings> settings,
    ILogger<TagOuterRangeCrawler> logger)
    : ScopedBackgroudService
{
    internal readonly PeriodicTimer PeriodicTimer = new PeriodicTimer(settings.Value.SubsequentRunDelay);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        do
        {
            var lastTag = await sqlDatabaseContext.ParsedTags
                .OrderBy(tag => tag.NumberId)
                .LastOrDefaultAsync(cancellationToken);
            if (lastTag == null)
            {
                logger.LogInformation("No tags found. Will try again later");
                continue;
            }

            for (var tagNumberId = lastTag.NumberId + 1; ; tagNumberId++)
            {
                var parsedTag = await tagCrawler.CrawlAsync(tagNumberId, cancellationToken);
                if (parsedTag == null)
                {
                    logger.LogInformation("No new last tag found. Will try again later");
                    break;
                }                    
            }
        } while (await PeriodicTimer.WaitForNextTickAsync(cancellationToken));
    }
}