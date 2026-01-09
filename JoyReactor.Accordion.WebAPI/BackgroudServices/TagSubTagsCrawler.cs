using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class TagSubTagsCrawler(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<TagSubTagsCrawler> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        var tagsWithEmptySubTags = (ParsedTag[])null;
        do
        {
            await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
            await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
            var tagCrawler = serviceScope.ServiceProvider.GetRequiredService<ITagCrawler>();

            tagsWithEmptySubTags = await sqlDatabaseContext.ParsedTags
                .AsNoTracking()
                .Where(tag => tag.MainTagId == null && tag.SubTagsCount > 0 && tag.SubTags.Count() < tag.SubTagsCount)
                .OrderByDescending(tag => tag.Id)
                .Take(100)
                .ToArrayAsync(cancellationToken);

            if (tagsWithEmptySubTags.Length != 0)
                logger.LogInformation("Crawling {TagsCount} tags for new sub tags.", tagsWithEmptySubTags.Count());
            else
            {
                logger.LogInformation("No tags without sub tags found. Will try again later.");
                continue;
            }

            foreach (var parsedTag in tagsWithEmptySubTags)
                await tagCrawler.CrawlSubTagsAsync(parsedTag.NumberId, cancellationToken);
        } while (tagsWithEmptySubTags.Length != 0);
    }
}