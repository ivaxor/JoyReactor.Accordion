using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.ApiClient.Constants;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JoyReactor.Accordion.Workers.HostedServices;

public class MainTagsWorker(
    IServiceScopeFactory serviceScopeFactory,
    ITagClient tagClient,
    ILogger<MainTagsWorker> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
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
            await Crawl(sqlDatabaseContext, tagName, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    internal async Task Crawl(SqlDatabaseContext sqlDatabaseContext, string tagName, CancellationToken cancellationToken)
    {
        var tag = await tagClient.GetByNameAsync(tagName, TagLineType.NEW, cancellationToken);
        var parsedTag = new ParsedTag(tag);

        await sqlDatabaseContext.ParsedTags.AddAsync(parsedTag, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
    }
}