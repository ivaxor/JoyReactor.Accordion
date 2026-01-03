using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JoyReactor.Accordion.Workers.BackgroudServices;

public class EmptySubTagsCrawler : BackgroundService, IDisposable
{
    internal readonly ITagClient tagClient;
    internal readonly ILogger<EmptySubTagsCrawler> logger;
    internal readonly IServiceScope serviceScope;
    internal readonly SqlDatabaseContext sqlDatabaseContext;

    public EmptySubTagsCrawler(
        IServiceScopeFactory serviceScopeFactory,
        ITagClient tagClient,
        ILogger<EmptySubTagsCrawler> logger)
    {
        this.tagClient = tagClient;
        this.logger = logger;

        serviceScope = serviceScopeFactory.CreateScope();
        sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
    }

    public void Dispose()
    {
        serviceScope.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(15));
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
                await CrawlAsync(parsedTag, cancellationToken);
            }
        } while (tagsWithEmptySubTags.Length != 0 || await periodicTimer.WaitForNextTickAsync(cancellationToken));
    }

    internal async Task CrawlAsync(ParsedTag parentTag, CancellationToken cancellationToken)
    {
        var subTags = await tagClient.GetAllSubTagsAsync(parentTag.NumberId, TagLineType.NEW, cancellationToken);
        var parsedSubTags = subTags
            .Select(subTag => new ParsedTag(subTag, parentTag))
            .ToArray();
        logger.LogInformation("Found {TagsCount} sub tags in \"{TagName}\" tag", parsedSubTags.Count(), parentTag.Name);

        if (parsedSubTags.Length == 0)
            return;

        await sqlDatabaseContext.ParsedTags.AddRangeIgnoreExistingAsync(parsedSubTags, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
    }
}