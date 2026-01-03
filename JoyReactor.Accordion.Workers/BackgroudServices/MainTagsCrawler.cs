using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.ApiClient.Constants;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JoyReactor.Accordion.Workers.BackgroudServices;

public class MainTagsCrawler(
    IServiceScopeFactory serviceScopeFactory,
    ITagClient tagClient,
    ILogger<MainTagsCrawler> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = serviceScopeFactory.CreateScope();
        using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();

        var existingMainTagNames = await sqlDatabaseContext.ParsedTags
           .Where(tagName => TagConstants.MainTags.ToArray().Contains(tagName.Name))
           .Select(tag => tag.Name)
           .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);
        var nonExistingMainTagNames = TagConstants.MainTags
            .Where(tagName => !existingMainTagNames.Contains(tagName))
            .ToArray();
        logger.LogInformation("Crawling {TagsCount} main category tags", nonExistingMainTagNames.Count());
        foreach (var tagName in nonExistingMainTagNames)
        {
            logger.LogInformation("Crawling \"{TagName}\" main category tag", tagName);
            await CrawlAsync(sqlDatabaseContext, tagName, cancellationToken);
        }
    }

    internal async Task CrawlAsync(SqlDatabaseContext sqlDatabaseContext, string tagName, CancellationToken cancellationToken)
    {
        var tag = await tagClient.GetByNameAsync(tagName, TagLineType.NEW, cancellationToken);
        var parsedTag = new ParsedTag(tag);

        await sqlDatabaseContext.ParsedTags.AddAsync(parsedTag, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
    }
}