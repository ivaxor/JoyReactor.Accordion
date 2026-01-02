using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JoyReactor.Accordion.Workers.HostedServices;

public class SubTagsWorker(
    IServiceScopeFactory serviceScopeFactory,
    ITagClient tagClient,
    ILogger<SubTagsWorker> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        var tagsWithEmptySubTags = (ParsedTag[])null;

        do
        {
            using var serviceScope = serviceScopeFactory.CreateScope();
            using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();

            tagsWithEmptySubTags = await sqlDatabaseContext.ParsedTags
                .Where(tag => tag.SubTagsCount > 0 && tag.SubTags.Count() < tag.SubTagsCount)
                .ToArrayAsync(cancellationToken);
            logger.LogInformation("Crawling {TagsCount} tags for sub tags", tagsWithEmptySubTags.Count());

            foreach (var parsedTag in tagsWithEmptySubTags)
            {
                logger.LogInformation("Crawling \"{TagName}\" tag for sub tags", parsedTag.Name);
                await Crawl(sqlDatabaseContext, parsedTag, cancellationToken);
            }
        } while (await periodicTimer.WaitForNextTickAsync(cancellationToken));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    internal async Task Crawl(SqlDatabaseContext sqlDatabaseContext, ParsedTag parentTag, CancellationToken cancellationToken)
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