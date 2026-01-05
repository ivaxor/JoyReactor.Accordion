using JoyReactor.Accordion.Logic.ApiClient.Constants;
using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class MainTagsCrawler(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<MainTagsCrawler> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
        await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
        var tagCrawler = serviceScope.ServiceProvider.GetRequiredService<ITagCrawler>();

        var existingMainTagNames = await sqlDatabaseContext.ParsedTags
            .AsNoTracking()
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