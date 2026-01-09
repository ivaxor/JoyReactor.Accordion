using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class TagOuterRangeCrawler(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<TagOuterRangeCrawler> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected override async Task RunAsync(CancellationToken cancellationToken)
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
            logger.LogInformation("No tags found. Will try again later.");
            return;
        }

        for (var tagNumberId = lastTag.NumberId + 1; ; tagNumberId++)
        {
            var parsedTag = await tagCrawler.CrawlAsync(tagNumberId, cancellationToken);
            if (parsedTag == null)
            {
                logger.LogInformation("No new last tag found. Will try again later.");
                break;
            }
        }
    }
}