using JoyReactor.Accordion.Logic.ApiClient.Constants;
using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class MainTagsCrawler(
    SqlDatabaseContext sqlDatabaseContext,
    ITagCrawler tagCrawler,
    ILogger<MainTagsCrawler> logger)
    : ScopedBackgroudService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var existingMainTagNames = await sqlDatabaseContext.ParsedTags
           .Where(tagName => TagConstants.MainTags.ToArray().Contains(tagName.Name))
           .Select(tag => tag.Name)
           .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);
        var nonExistingMainTagNames = TagConstants.MainTags
            .Where(tagName => !existingMainTagNames.Contains(tagName))
            .ToArray();

        if (nonExistingMainTagNames.Length == 0)
        {
            logger.LogInformation("No new main category tags found");
            return;
        }
        logger.LogInformation("Crawling {TagsCount} main category tags", nonExistingMainTagNames.Count());

        foreach (var tagName in nonExistingMainTagNames)
            await tagCrawler.CrawlAsync(tagName, cancellationToken);
    }
}