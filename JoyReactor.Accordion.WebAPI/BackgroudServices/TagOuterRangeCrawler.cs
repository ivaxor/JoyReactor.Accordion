using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class TagOuterRangeCrawler(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<CrawlerSettings> settings,
    ILogger<TagOuterRangeCrawler> logger)
    : BackgroundService
{
    internal readonly PeriodicTimer PeriodicTimer = new PeriodicTimer(settings.Value.SubsequentRunDelay);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        do
        {
            await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
            await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
            var tagCrawler = serviceScope.ServiceProvider.GetRequiredService<ITagCrawler>();

            var lastTag = await sqlDatabaseContext.ParsedTags
                .AsNoTracking()
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