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

public class TagCrawlerWorker : IHostedService
{
    internal readonly SqlDatabaseContext sqlDatabaseContext;
    internal readonly ITagClient tagClient;
    internal readonly ILogger<TagCrawlerWorker> logger;

    public TagCrawlerWorker(
        IServiceScopeFactory serviceScopeFactory,
        ITagClient tagClient,
        ILogger<TagCrawlerWorker> logger)
    {
        var serviceScope = serviceScopeFactory.CreateScope();
        sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();

        this.tagClient = tagClient;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        var existingMainTagNames = await sqlDatabaseContext.ParsedTags
            .Where(tagName => TagConstants.MainCategories.ToArray().Contains(tagName.Name))
            .Select(tag => tag.Name)
            .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);
        var nonExistingMainTagNames = TagConstants.MainCategories
            .Where(tagName => !existingMainTagNames.Contains(tagName))
            .ToArray();
        logger.LogInformation("Crawling {TagsCount} main category tags", nonExistingMainTagNames.Count());
        foreach (var tagName in nonExistingMainTagNames)
        {
            logger.LogInformation("Crawling \"{TagName}\" main category tag", tagName);
            await Crawl(tagName, cancellationToken);
        }

        var tagsWithEmptySubTags = (ParsedTag[])null;
        do
        {
            tagsWithEmptySubTags = await sqlDatabaseContext.ParsedTags
                .Where(tag => tag.SubTagsCount > 0 && tag.SubTags.Count() < tag.SubTagsCount)
                .ToArrayAsync(cancellationToken);
            logger.LogInformation("Crawling {TagsCount} tags for sub tags", tagsWithEmptySubTags.Count());

            foreach (var parsedTag in tagsWithEmptySubTags)
            {
                logger.LogInformation("Crawling \"{TagName}\" tag for sub tags", parsedTag.Name);
                await Crawl(parsedTag, cancellationToken);
            }
        } while (tagsWithEmptySubTags.Length != 0);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    internal async Task Crawl(string tagName, CancellationToken cancellationToken)
    {
        var tag = await tagClient.GetByNameAsync(tagName, TagLineType.NEW, cancellationToken);
        var parsedTag = new ParsedTag(tag);

        await sqlDatabaseContext.ParsedTags.AddAsync(parsedTag, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
    }

    internal async Task Crawl(ParsedTag parentTag, CancellationToken cancellationToken)
    {
        var subTags = await tagClient.GetAllSubTagsAsync(parentTag.NumberId, TagLineType.NEW, cancellationToken);
        var parsedSubTags = subTags
            .Select(subTag => new ParsedTag(subTag, parentTag))
            .ToArray();

        var parsedSubTagIds = parsedSubTags
            .Select(subTag => subTag.Id)
            .ToArray();
        var existingTagIds = await sqlDatabaseContext.ParsedTags
            .Where(subTag => parsedSubTagIds.Contains(subTag.Id))
            .Select(subTag => subTag.Id)
            .ToHashSetAsync(cancellationToken);

        parsedSubTags = parsedSubTags
            .Where(subTag => !existingTagIds.Contains(subTag.Id))
            .ToArray();
        logger.LogInformation("Found {TagsCount} new sub tags in \"{TagName}\" tag", parsedSubTags.Count(), parentTag.Name);

        await sqlDatabaseContext.ParsedTags.AddRangeAsync(parsedSubTags, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
    }
}